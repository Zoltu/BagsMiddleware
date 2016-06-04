using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Zoltu.BagsMiddleware.Amazon;
using Zoltu.BagsMiddleware.Extensions;
using Zoltu.BagsMiddleware.Models;

namespace Zoltu.BagsMiddleware.Controllers
{
	[Route("api/products")]
	public class ProductsController : Controller
    {
		private BagsContext _bagsContext;
		private RequestSigner _requestSigner;
		private String _associateTag;

		public ProductsController(BagsContext bagsContext, IConfiguration configuration)
		{
			_bagsContext = bagsContext;

			_associateTag = configuration["AmazonAssociateTag"];
			if (_associateTag == null) throw new Exception("AmazonAssociateTag is missing.");

			var accessKeyId = configuration["AmazonAccessKeyId"];
			if (accessKeyId == null) throw new Exception("AmazonAccessKeyId");

			var secretAccessKey = configuration["AmazonSecretAccessKey"];
			if (secretAccessKey == null) throw new Exception("AmazonSecretAccessKey");

			_requestSigner = new RequestSigner(accessKeyId, secretAccessKey, "ecs.amazonaws.com");
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> GetProducts()
		{
			return HttpResult.Ok(await _bagsContext.Products
				.WithUnsafeIncludes()
				.AsAsyncEnumerable()
				.Select(product => product.ToUnsafeExpandedWireFormat())
				.ToList());
		}

		[HttpGet]
		[Route("{product_id:guid}")]
		public async Task<IActionResult> GetProduct([FromRoute(Name = "product_id")] Guid productId)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// locate product
			var foundProduct = await _bagsContext.Products
				.WithUnsafeIncludes()
				.Where(product => product.Id == productId)
				.SingleOrDefaultAsync();
			if (foundProduct == null)
				return HttpResult.NotFound();

			return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat());
		}

		[HttpGet]
		[Route("by_tags")]
		public async Task<IActionResult> GetProductsByTags([FromQuery(Name = "tag_id")] IEnumerable<Guid> tagIds)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			var tagIdsList = tagIds.ToList();

			var query = @"
SELECT product.Id as Id, product.Name as Name, product.Price as Price
	FROM ProductTag productTag0
	";
			query += String.Join("\r\n	", tagIds
				.Select((guid, i) => new { guid, i })
				.Skip(1)
				.Select(item => $"JOIN ProductTag productTag{item.i} ON productTag0.ProductId = productTag{item.i}.ProductId"));
			query += @"
	JOIN Product product ON productTag0.ProductId = product.Id
	";
			if (tagIds.Count() != 0)
				query += "WHERE ";
			query += String.Join("\r\n		AND ", tagIds
				.Select((guid, i) => new { guid, i })
				.Select(item => $"productTag{item.i}.TagId = @p{item.i}"));

			// locate matching products
			var matchingProducts = _bagsContext.Products
				.WithUnsafeIncludes()
				.FromSql(query, tagIds.Select(guid => guid as Object).ToArray())
				.Select(product => new
				{
					id = product.Id,
					name = product.Name,
					price = product.Price,
					image_urls = product.ImageUrls.Select(imageUrl => imageUrl.Url),
					purchase_urls = product.PurchaseUrls.Select(purchaseUrl => purchaseUrl.Url),
					tags = product.Tags.Select(productTag => productTag.Tag).Select(tag => new
					{
						id = tag.Id,
						name = tag.Name,
						category = new
						{
							id = tag.TagCategory.Id,
							name = tag.TagCategory.Name
						}
					})
				})
				// FIXME: This should be `ToListAsync` or `AsAsyncEnumerable` https://github.com/aspnet/EntityFramework/issues/5640
				.ToList();

			return HttpResult.Ok(matchingProducts);
		}

		public class CreateProductRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "name")]
			public String Name { get; set; }
			[JsonProperty(Required = Required.Always, PropertyName = "price")]
			public UInt32 Price { get; set; }
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// create product
			var newProduct = new Models.Product { Name = request.Name, Price = request.Price };
			_bagsContext.Products.Add(newProduct);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(newProduct.ToSafeExpandedWireFormat());
		}

		public class CreateProductFromAsinRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "asin")]
			public String Asin { get; set; }
		}

		[HttpPut]
		[Route("amazon")]
		public async Task<IActionResult> CreateProduct([FromBody] CreateProductFromAsinRequest request)
		{
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			var requestParameters = new Dictionary<String, String>
			{
				{ "IdType", "ASIN" },
				{ "Operation", "ItemLookup" },
				{ "ResponseGroup", "Images,Offers,ItemAttributes" },
				{ "Service", "AWSECommerceService" },
				{ "AssociateTag", _associateTag },
				{ "ItemId", request.Asin }
			};

			var amazonRequestUri = _requestSigner.Sign(requestParameters);
			var httpClient = new HttpClient();
			var result = await httpClient.GetStringAsync(amazonRequestUri);

			var xElement = XElement.Parse(result);
			XNamespace ns = "http://webservices.amazon.com/AWSECommerceService/2011-08-01";
			var item = xElement
				.Elements(ns + "Items")
				.Single()
				.Elements(ns + "Item")
				.Single();

			var primaryImage = item
				.Elements(ns + "LargeImage")
				.Single()
				.Elements(ns + "URL")
				.Single()
				.Value;

			var images = item
				.Elements(ns + "ImageSets")
				.Single()
				.Elements(ns + "ImageSet")
				.Where(imageSet => imageSet.Attribute("Category")?.Value != "primary")
				.Select(imageSet => imageSet
					.Elements(ns + "LargeImage")
					.Single()
					.Elements(ns + "URL")
					.Single()
					.Value);
			images = new[] { primaryImage }
				.Concat(images)
				.Select(image => new UriBuilder(image) { Scheme = "https", Port = -1 }.ToString());

			var lowestNewPrice = Double.Parse(item
				.Elements(ns + "OfferSummary")
				.Single()
				.Elements(ns + "LowestNewPrice")
				.Single()
				.Elements(ns + "Amount")
				.Single()
				.Value) / 100;

			var title = item
				.Elements(ns + "ItemAttributes")
				.Single()
				.Elements(ns + "Title")
				.Single()
				.Value;

			var affiliateLink = $"https://www.amazon.com/gp/product/{request.Asin}/?tag={_associateTag}";

			// create product
			var newProduct = new Models.Product { Name = title, Price = Convert.ToInt64(lowestNewPrice) };
			var newImageUrls = images.Select(imageUrl => new Models.ProductImageUrl { Product = newProduct, Url = imageUrl });
			var newPurchaseUrl = new Models.ProductPurchaseUrl { Product = newProduct, Url = affiliateLink };
			newProduct.ImageUrls.AddRange(newImageUrls);
			newProduct.PurchaseUrls.Add(newPurchaseUrl);
			_bagsContext.Products.Add(newProduct);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(newProduct.ToSafeExpandedWireFormat());
		}

		public class EditProductRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "name")]
			public String Name { get; set; }
			[JsonProperty(Required = Required.Always, PropertyName = "price")]
			public UInt32 Price { get; set; }
		}

		[HttpPut]
		[Route("{product_id:guid}")]
		public async Task<IActionResult> EditProduct([FromRoute(Name = "product_id")] Guid productId, [FromBody] EditProductRequest request)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// locate product
			var foundProduct = await _bagsContext.Products
				.WithUnsafeIncludes()
				.Where(product => product.Id == productId)
				.SingleOrDefaultAsync();
			if (foundProduct == null)
				return HttpResult.NotFound($"{productId}");

			// verify there are changes
			if (foundProduct.Name == request.Name && foundProduct.Price == request.Price)
				return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat());

			// change name/price
			foundProduct.Name = request.Name;
			foundProduct.Price = request.Price;
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat());
		}

		[HttpDelete]
		[Route("{product_id:guid}")]
		public async Task<IActionResult> DeleteProduct([FromRoute(Name = "product_id")] Guid productId)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// locate product
			var foundProduct = await _bagsContext.Products
				.Where(product => product.Id == productId)
				.SingleOrDefaultAsync();
			if (foundProduct == null)
				return HttpResult.NoContent();

			// delete
			_bagsContext.Products.Remove(foundProduct);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.NoContent();
		}

		public class AddTagRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "tag_id")]
			public Guid TagId { get; set; }
		}

		[HttpPut]
		[Route("{product_id:guid}/tag")]
		public async Task<IActionResult> AddTag([FromRoute(Name = "product_id")] Guid productId, [FromBody] AddTagRequest request)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// validate product
			var foundProduct = await _bagsContext.Products
				.WithUnsafeIncludes()
				.Where(product => product.Id == productId)
				.SingleOrDefaultAsync();
			if (foundProduct == null)
				return HttpResult.NotFound($"{productId}");

			// validate tag
			var foundTag = await _bagsContext.Tags
				.WithSafeIncludes()
				.Where(tag => tag.Id == request.TagId)
				.SingleOrDefaultAsync();
			if (foundTag == null)
				return HttpResult.NotFound($"{request.TagId}");

			// link tag and product
			var productTag = new Models.ProductTag { Product = foundProduct, Tag = foundTag };
			_bagsContext.ProductTags.Add(productTag);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat());
		}

		[HttpDelete]
		[Route("{product_id:guid}/tag/{tag_id:guid}")]
		public async Task<IActionResult> RemoveTag([FromRoute(Name = "product_id")] Guid productId, [FromRoute(Name = "tag_id")] Guid tagId)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// validate product
			var foundProduct = await _bagsContext.Products
				.Where(product => product.Id == productId)
				.SingleOrDefaultAsync();
			if (foundProduct == null)
				return HttpResult.NotFound($"{productId}");

			// validate tag
			var foundTag = await _bagsContext.Tags
				.Where(tag => tag.Id == tagId)
				.SingleOrDefaultAsync();
			if (foundTag == null)
				return HttpResult.NotFound($"{tagId}");

			// locate product tag
			var foundProductTag = await _bagsContext.ProductTags
				.Where(productTag => productTag.ProductId == foundProduct.Id && productTag.TagId == foundTag.Id)
				.SingleOrDefaultAsync();
			if (foundProductTag == null)
				return HttpResult.NoContent();

			// delete
			_bagsContext.ProductTags.Remove(foundProductTag);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.NoContent();
		}

		public class AddImageUrlRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "uri")]
			public Uri ImageUrl { get; set; }
		}

		[HttpPut]
		[Route("{product_id:guid}/image_url")]
		public async Task<IActionResult> AddImageUrl([FromRoute(Name = "product_id")] Guid productId, [FromBody] AddImageUrlRequest request)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// validate product
			var foundProduct = await _bagsContext.Products
				.WithUnsafeIncludes()
				.Where(product => product.Id == productId)
				.SingleOrDefaultAsync();
			if (foundProduct == null)
				return HttpResult.NotFound();

			// create linked image URL
			_bagsContext.ProductImageUrls.Add(new Models.ProductImageUrl { Product = foundProduct, Url = request.ImageUrl.ToString() });
			await _bagsContext.SaveChangesAsync();

			return Ok(foundProduct.ToUnsafeExpandedWireFormat());
		}

		[HttpDelete]
		[Route("{product_id:guid}/image_url")]
		public async Task<IActionResult> RemoveImageUrl([FromRoute(Name = "product_id")] Guid productId, [FromQuery(Name = "image_url")] Uri imageUrl)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// validate product
			var foundProduct = await _bagsContext.Products
				.Include(product => product.ImageUrls)
				.Where(product => product.Id == productId)
				.SingleOrDefaultAsync();
			if (foundProduct == null)
				return HttpResult.NotFound();

			// locate image URL
			var foundProductImages = foundProduct.ImageUrls
				.Where(productImageUrl => productImageUrl.Url == imageUrl.ToString());

			// remove the matching product images
			_bagsContext.ProductImageUrls.RemoveRange(foundProductImages);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.NoContent();
		}

		public class AddPurchaseUrlRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "uri")]
			public Uri PurchaseUrl { get; set; }
		}

		[HttpPut]
		[Route("{product_id:guid}/purchase_url")]
		public async Task<IActionResult> AddPurchaseUrl([FromRoute(Name = "product_id")] Guid productId, [FromBody] AddPurchaseUrlRequest request)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// validate product
			var foundProduct = await _bagsContext.Products
				.WithUnsafeIncludes()
				.Where(product => product.Id == productId)
				.SingleOrDefaultAsync();
			if (foundProduct == null)
				return HttpResult.NotFound();

			// create linked purchase URL
			_bagsContext.ProductPurchaseUrls.Add(new Models.ProductPurchaseUrl { Product = foundProduct, Url = request.PurchaseUrl.ToString() });
			await _bagsContext.SaveChangesAsync();

			return Ok(foundProduct.ToUnsafeExpandedWireFormat());
		}

		[HttpDelete]
		[Route("{product_id:guid}/purchase_url")]
		public async Task<IActionResult> RemovePurchaseUrl([FromRoute(Name = "product_id")] Guid productId, [FromQuery(Name = "purchase_url")] Uri purchaseUrl)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// validate product
			var foundProduct = await _bagsContext.Products
				.Include(product => product.PurchaseUrls)
				.Where(product => product.Id == productId)
				.SingleOrDefaultAsync();
			if (foundProduct == null)
				return HttpResult.NotFound();

			// locate image URL
			var foundProductPurchaseUrl = foundProduct.PurchaseUrls
				.Where(productPurchaseUrl => productPurchaseUrl.Url == purchaseUrl.ToString());

			// remove the matching product images
			_bagsContext.ProductPurchaseUrls.RemoveRange(foundProductPurchaseUrl);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.NoContent();
		}
	}
}
