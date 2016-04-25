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
				.Select(product => new
				{
					id = product.Id,
					name = product.Name,
					price = product.Price,
					tags = product.Tags.Select(productTag => productTag.Tag).Select(tag => new
					{
						id = tag.Id,
						name = tag.Name,
						category_id = tag.TagCategory.Id,
						category_name = tag.TagCategory.Name
					})
				})
				.ToListAsync());
		}

		[HttpGet]
		[Route("by_tags")]
		public async Task<IActionResult> GetProductsByTags([FromQuery(Name = "tag_id")] IEnumerable<Guid> expectedTagIds)
		{
			Task.WaitAll(_bagsContext.ProductTags.LoadAsync(), _bagsContext.Products.LoadAsync());
			return Ok(await _bagsContext.Products
				.Where(product => expectedTagIds
					.All(expectedTagId => product.Tags
						.Select(productTag => productTag.TagId)
						.Contains(expectedTagId)))
				.Select(product => new
				{
					id = product.Id,
					name = product.Name,
					price = product.Price,
					tags = product.Tags.Select(productTag => productTag.Tag).Select(tag => new
					{
						id = tag.Id,
						name = tag.Name,
						category_id = tag.TagCategory.Id,
						category_name = tag.TagCategory.Name
					})
				})
				.ToListAsync());
		}

		[HttpPut]
		[Route("add_tag")]
		public async Task<IActionResult> AddTag([FromQuery(Name = "product_id")] Guid productId, [FromQuery(Name = "tag_id")] Guid tagId)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest();

			var tagTask = _bagsContext.Tags.Where(tag => tag.Id == tagId).SingleAsync();
			var productTask = _bagsContext.Products.Where(product => product.Id == productId).SingleAsync();
			var foundTag = await tagTask;
			var foundProduct = await productTask;
			var productTag = new Models.ProductTag { ProductId = foundProduct.Id, TagId = foundTag.Id };
			_bagsContext.ProductTags.Add(productTag);
			await _bagsContext.SaveChangesAsync();

			return new NoContentResult();
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateProduct([FromQuery(Name = "name")] String name, [FromQuery] UInt32 price, [FromQuery(Name = "image_url")] IEnumerable<String> imageUrls, [FromQuery(Name = "purchase_url")] IEnumerable<String> purchaseUrls)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest();

			var newProduct = _bagsContext.Products.Add(new Models.Product { Name = name, Price = price });
			await _bagsContext.SaveChangesAsync();

			return new NoContentResult();
		}
	}
}
