using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
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
			return Ok(await _bagsContext.Products
				.WithIncludes()
				.AsAsyncEnumerable()
				.Select(product => product.ToExpandedWireFormat())
				.ToList());
		}

		[HttpGet]
		[Route("by_tags")]
		public async Task<IActionResult> GetProductsByTags([FromQuery(Name = "tag_id")] IEnumerable<Guid> expectedTagIds)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest(ModelState);

			// FIXME: http://stackoverflow.com/questions/36834629/why-do-i-need-to-call-loadasync-before-querying-over-the-same
			await _bagsContext.Products.Include(product => product.Tags).LoadAsync();

			return Ok(await _bagsContext.Products
				.Where(product => expectedTagIds
					.All(expectedTagId => product.Tags
						.Select(productTag => productTag.TagId)
						.Contains(expectedTagId)))
				.WithIncludes()
				.AsAsyncEnumerable()
				.Select(product => product.ToExpandedWireFormat())
				.ToList());
		}

		[HttpPut]
		[Route("add_tag")]
		public async Task<IActionResult> AddTag([FromQuery(Name = "product_id")] Guid productId, [FromQuery(Name = "tag_id")] Guid tagId)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest(ModelState);

			var productTag = new Models.ProductTag { ProductId = productId, TagId = tagId };
			_bagsContext.ProductTags.Add(productTag);
			await _bagsContext.SaveChangesAsync();

			var updatedProduct = await _bagsContext.Products
				.WithIncludes()
				.Where(product => product.Id == productId)
				.SingleAsync();
			return Ok(updatedProduct.ToExpandedWireFormat());
		}

		[HttpPut]
		[Route("add_image_url")]
		public async Task<IActionResult> AddImageUrl([FromQuery(Name = "product_id")] Guid productId, [FromQuery(Name = "image_url")] Uri imageUrl)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest(ModelState);

			var foundProduct = await _bagsContext.Products
				.WithIncludes()
				.Where(product => product.Id == productId)
				.SingleAsync();
			_bagsContext.ProductImageUrls.Add(new Models.ProductImageUrl { ProductId = foundProduct.Id, Url = imageUrl.ToString() });
			await _bagsContext.SaveChangesAsync();

			return Ok(foundProduct.ToExpandedWireFormat());
		}

		[HttpPut]
		[Route("add_purchase_url")]
		public async Task<IActionResult> AddPurchaseUrl([FromQuery(Name = "product_id")] Guid productId, [FromQuery(Name = "purchase_url")] Uri purchaseUrl)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest(ModelState);

			var foundProduct = await _bagsContext.Products
				.WithIncludes()
				.Where(product => product.Id == productId)
				.SingleAsync();
			_bagsContext.ProductPurchaseUrls.Add(new Models.ProductPurchaseUrl { ProductId = foundProduct.Id, Url = purchaseUrl.ToString() });
			await _bagsContext.SaveChangesAsync();

			return Ok(foundProduct.ToExpandedWireFormat());
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateProduct([FromQuery(Name = "name")] String name, [FromQuery] UInt32 price)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest(ModelState);

			var newProduct = new Models.Product { Name = name, Price = price };
			_bagsContext.Products.Add(newProduct);
			await _bagsContext.SaveChangesAsync();

			return Ok(newProduct.ToExpandedWireFormat());
		}
	}
}
