using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Data.Entity;
using Newtonsoft.Json;
using Zoltu.BagsMiddleware.Extensions;
using Zoltu.BagsMiddleware.Models;

namespace Zoltu.BagsMiddleware.Controllers
{
	[Route("api/products")]
	public class ProductsController : Controller
    {
		private BagsContext _bagsContext;

		public ProductsController(BagsContext bagsContext)
		{
			_bagsContext = bagsContext;
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

			// FIXME: http://stackoverflow.com/questions/36834629/why-do-i-need-to-call-loadasync-before-querying-over-the-same
			await _bagsContext.Products
				.Include(product => product.Tags)
				.LoadAsync();

			// locate matching products
			var matchingProducts = await _bagsContext.Products
				.WithSafeIncludes()
				.Where(product => tagIds
					.All(expectedTagId => product.Tags
						.Select(productTag => productTag.TagId)
						.Contains(expectedTagId)))
				.AsAsyncEnumerable()
				.Select(product => product.ToSafeExpandedWireFormat())
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
		[Route("{product_id:guid}/image_url/{image_url}")]
		public async Task<IActionResult> RemoveImageUrl([FromRoute(Name = "product_id")] Guid productId, [FromRoute(Name = "image_url")] String imageUrlString)
		{
			// FIXME: figure out how to get the image_url from the route as a decoded URI 
			var imageUrl = new Uri(WebUtility.UrlDecode(imageUrlString));

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
		[Route("{product_id:guid}/purchase_url/{purchase_url}")]
		public async Task<IActionResult> RemovePurchaseUrl([FromRoute(Name = "product_id")] Guid productId, [FromRoute(Name = "purchase_url")] String purchaseUrlString)
		{
			// FIXME: figure out how to get the image_url from the route as a decoded URI 
			var purchaseUrl = new Uri(WebUtility.UrlDecode(purchaseUrlString));

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
