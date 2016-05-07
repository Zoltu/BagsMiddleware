using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace Zoltu.BagsMiddleware.Controllers
{
	[Route("api/products")]
	public class ProductsController : Controller
    {
		private Models.BagsContext _bagsContext;

		public ProductsController(Models.BagsContext bagsContext)
		{
			_bagsContext = bagsContext;
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> GetProducts()
		{
			return Ok(await _bagsContext.Products
				.Select(product => product.ToExpandedWireFormat())
				.ToListAsync());
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
				.Select(product => product.ToExpandedWireFormat())
				.ToListAsync());
		}

		[HttpPut]
		[Route("add_tag")]
		public async Task<IActionResult> AddTag([FromQuery(Name = "product_id")] Guid productId, [FromQuery(Name = "tag_id")] Guid tagId)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest(ModelState);

			var tagTask = _bagsContext.Tags.Where(tag => tag.Id == tagId).SingleAsync();
			var productTask = _bagsContext.Products.Where(product => product.Id == productId).SingleAsync();
			var foundTag = await tagTask;
			var foundProduct = await productTask;
			var productTag = new Models.ProductTag { ProductId = foundProduct.Id, TagId = foundTag.Id };
			_bagsContext.ProductTags.Add(productTag);
			await _bagsContext.SaveChangesAsync();

			return Ok(foundProduct.ToExpandedWireFormat());
		}

		[HttpPut]
		[Route("add_image_url")]
		public async Task<IActionResult> AddImageUrl([FromQuery(Name = "product_id")] Guid productId, [FromQuery(Name = "image_url")] Uri imageUrl)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest(ModelState);

			var foundProduct = await _bagsContext.Products.Where(product => product.Id == productId).SingleAsync();
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

			var foundProduct = await _bagsContext.Products.Where(product => product.Id == productId).SingleAsync();
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
