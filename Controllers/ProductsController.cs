﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Zoltu.Bags.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Zoltu.Bags.Api.Amazon;
using Zoltu.Bags.Api.Models;

namespace Zoltu.Bags.Api.Controllers
{
    [Route("api/products")]
	public class ProductsController : Controller
	{
		private BagsContext _bagsContext;
		private AmazonUtilities _amazon;

		public ProductsController(BagsContext bagsContext, IConfiguration configuration)
		{
			_bagsContext = bagsContext;

			var associateTag = configuration["AmazonAssociateTag"];
			if (associateTag == null) throw new Exception("AmazonAssociateTag is missing.");

			var accessKeyId = configuration["AmazonAccessKeyId"];
			if (accessKeyId == null) throw new Exception("AmazonAccessKeyId");

			var secretAccessKey = configuration["AmazonSecretAccessKey"];
			if (secretAccessKey == null) throw new Exception("AmazonSecretAccessKey");

			_amazon = new AmazonUtilities(accessKeyId, secretAccessKey, associateTag);
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> GetProducts()
		{
			var products = await _bagsContext.Products
				.WithUnsafeIncludes()
				.ToListAsync();
			var serializableProducts = products
				.Select(product => product.ToUnsafeExpandedWireFormat(_amazon))
				.ToList();
			return HttpResult.Ok(serializableProducts);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route("{product_id:int}")]
		public async Task<IActionResult> GetProduct([FromRoute(Name = "product_id")] Int32 productId)
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

			return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat(_amazon));
		}

		[HttpGet]
		[AllowAnonymous]
		[Route("by_tags")]
		public async Task<IActionResult> GetProductsByTags([FromQuery(Name = "tag_id")] IEnumerable<Guid> tagIds, [FromQuery(Name = "starting_product_id")] UInt16 startingId = 0, [FromQuery(Name = "products_per_page")] UInt16 itemsPerPage = 10, [FromQuery(Name = "min_price")] UInt32 minPrice = 0, [FromQuery(Name = "max_price")] UInt32 maxPrice = Int32.MaxValue)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			var tagIdsList = tagIds.ToList();

			// FIXME: typecast necessary until https://github.com/aspnet/EntityFramework/issues/5663 is fixed
			Int32 startingId32 = startingId;
			Int32 minPriceSigned = (Int32)Math.Min(minPrice, Int32.MaxValue);
			Int32 maxPriceSigned = (Int32)Math.Min(maxPrice, Int32.MaxValue);
			UInt32 count = (UInt32)Math.Max(0, tagIdsList.Count);

			var query = $@"
SELECT DISTINCT TOP({itemsPerPage}) products.Id as Id, products.Name as Name, products.Price as Price, products.ImagesJson as ImagesJson, products.Asin as Asin
FROM Products products
LEFT OUTER JOIN AmazonProducts amazonProducts ON products.Id = amazonProducts.ProductId";
			if (count > 0)
			{
				query += @"
	INNER JOIN (
		SELECT ProductId
		FROM ProductTags
		WHERE TagId IN (" + String.Join(", ", tagIds.Select((guid, i) => $"@p{i}")) + $@")
		GROUP BY ProductId
		HAVING COUNT(*) = {count}
	) AS tags ON tags.ProductId = products.Id";
			}
			query += $@"
WHERE amazonProducts.Price BETWEEN {minPriceSigned} AND {maxPriceSigned} AND products.Id >= {startingId} AND amazonProducts.Available = 1";

			// locate matching products
			var matchingProducts = _bagsContext.Products
				.WithUnsafeIncludes()
				.FromSql(query, tagIds.Select(guid => guid as Object).ToArray())
				// FIXME: This should be `ToListAsync` or `AsAsyncEnumerable` https://github.com/aspnet/EntityFramework/issues/5640
				.ToList()
				.Select(product => product.ToUnsafeExpandedWireFormat(_amazon))
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

			// see if the product already exists
			var foundProduct = await _bagsContext.Products
				.WithUnsafeIncludes()
				.Where(product => product.AmazonProduct.Asin == request.Asin)
				.FirstOrDefaultAsync();
			if (foundProduct != null)
				return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat(_amazon));

			var result = await _amazon.GetProductDetailsXml(request.Asin);
			var xElement = XElement.Parse(result);
			XNamespace ns = "http://webservices.amazon.com/AWSECommerceService/2011-08-01";
			var item = xElement
				.Elements(ns + "Items")
				.Single()
				.Elements(ns + "Item")
				.Single();

			var images = item
				.Elements(ns + "ImageSets")
				.Single()
				.Elements(ns + "ImageSet")
				.DistinctBy(imageSet => imageSet.Elements(ns + "LargeImage").Single().Elements(ns + "URL").Single().Value)
				.Select(imageSet => new Product.Image
				{
					Priority = (imageSet.Attribute("Category")?.Value == "primary") ? 10U : 100U,
					Small = _amazon.ConvertImageLinkToHttps(imageSet.Elements(ns + "SmallImage").Single().Elements(ns + "URL").Single().Value),
					Medium = _amazon.ConvertImageLinkToHttps(imageSet.Elements(ns + "MediumImage").Single().Elements(ns + "URL").Single().Value),
					Large = _amazon.ConvertImageLinkToHttps(imageSet.Elements(ns + "LargeImage").Single().Elements(ns + "URL").Single().Value)
				})
				.OrderBy(image => image.Priority);

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

			// create product
			var newProduct = new Models.Product
			{
				Name = title,
				Price = Convert.ToInt64(lowestNewPrice),
				Asin = request.Asin,
				AmazonProduct = new AmazonProduct { Asin = request.Asin, Available = true, LastChecked = DateTimeOffset.UtcNow, Price = Convert.ToInt32(lowestNewPrice) },
				ImagesJson = JsonConvert.SerializeObject(images)
			};
			_bagsContext.Products.Add(newProduct);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(newProduct.ToUnsafeExpandedWireFormat(_amazon));
		}

		public class EditProductRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "name")]
			public String Name { get; set; }
			[JsonProperty(Required = Required.Always, PropertyName = "price")]
			public UInt32 Price { get; set; }
			[JsonProperty(Required = Required.Default, PropertyName = "asin")]
			public String Asin { get; set; }
			[JsonProperty(Required = Required.Default, PropertyName = "images")]
			public IEnumerable<Product.Image> Images { get; set; }
		}

		[HttpPut]
		[Route("{product_id:int}")]
		public async Task<IActionResult> EditProduct([FromRoute(Name = "product_id")] Int32 productId, [FromBody] EditProductRequest request)
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

			// update the product
			foundProduct.Name = request.Name;
			if (request.Images != null)
				foundProduct.ImagesJson = JsonConvert.SerializeObject(request.Images);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat(_amazon));
		}

		[HttpDelete]
		[Route("{product_id:int}")]
		public async Task<IActionResult> DeleteProduct([FromRoute(Name = "product_id")] Int32 productId)
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
			[JsonProperty(Required = Required.Always, PropertyName = "tag_ids")]
			public IEnumerable<Guid> TagIds { get; set; }
		}

		[HttpPut]
		[Route("{product_id:int}/tag")]
		public async Task<IActionResult> AddTag([FromRoute(Name = "product_id")] Int32 productId, [FromBody] AddTagRequest request)
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

			// validate tags
			var foundTags = await _bagsContext.Tags
				.WithSafeIncludes()
				.Where(tag => request.TagIds.Contains(tag.Id))
				.ToListAsync();
			foreach (var expectedTagId in request.TagIds)
				if (!foundTags.Any(foundTag => foundTag.Id == expectedTagId))
					return HttpResult.NotFound($"{expectedTagId}");

			// link tag and product
			var preexistingTags = foundProduct.Tags.Select(productTag => productTag.Tag).ToList();
			var productTags = foundTags
				.Where(foundTag => !preexistingTags.Contains(foundTag))
				.Select(foundTag => new Models.ProductTag { Product = foundProduct, Tag = foundTag })
				.ToList();
			_bagsContext.ProductTags.AddRange(productTags);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat(_amazon));
		}

		[HttpDelete]
		[Route("{product_id:int}/tag/{tag_id:guid}")]
		public async Task<IActionResult> RemoveTag([FromRoute(Name = "product_id")] Int32 productId, [FromRoute(Name = "tag_id")] Guid tagId)
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
	}
}
