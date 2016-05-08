using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
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
				.WithSafeIncludes()
				.AsAsyncEnumerable()
				.Select(product => product.ToSafeExpandedWireFormat())
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
		public async Task<IActionResult> GetProductsByTags([FromQuery(Name = "tag_id")] IEnumerable<Guid> expectedTagIds)
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
				.Where(product => expectedTagIds
					.All(expectedTagId => product.Tags
						.Select(productTag => productTag.TagId)
						.Contains(expectedTagId)))
				.AsAsyncEnumerable()
				.Select(product => product.ToSafeExpandedWireFormat())
				.ToList();

			return HttpResult.Ok(matchingProducts);
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateProduct([FromQuery(Name = "name")] String name, [FromQuery] UInt32 price)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// create product
			var newProduct = new Models.Product { Name = name, Price = price };
			_bagsContext.Products.Add(newProduct);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(newProduct.ToSafeExpandedWireFormat());
		}

		[HttpPut]
		[Route("{product_id:guid}")]
		public async Task<IActionResult> EditProduct([FromRoute(Name = "product_id")] Guid productId, [FromQuery(Name = "name")] String name, [FromQuery] UInt32 price)
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
			if (foundProduct.Name == name && foundProduct.Price == price)
				return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat());

			// change name/price
			foundProduct.Name = name;
			foundProduct.Price = price;
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

		[HttpPut]
		[Route("{product_id:guid}/tag/${tag_id:guid}")]
		public async Task<IActionResult> AddTag([FromRoute(Name = "product_id")] Guid productId, [FromRoute(Name = "tag_id")] Guid tagId)
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
				.Where(tag => tag.Id == tagId)
				.SingleOrDefaultAsync();
			if (foundTag == null)
				return HttpResult.NotFound($"{tagId}");

			// link tag and product
			var productTag = new Models.ProductTag { Product = foundProduct, Tag = foundTag };
			_bagsContext.ProductTags.Add(productTag);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(foundProduct.ToUnsafeExpandedWireFormat());
		}

		[HttpDelete]
		[Route("{product_id:guid}/tag/${tag_id:guid}")]
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
				.Where(productTag => productTag.Product == foundProduct && productTag.Tag == foundTag)
				.SingleOrDefaultAsync();
			if (foundProductTag == null)
				return HttpResult.NoContent();

			// delete
			_bagsContext.ProductTags.Remove(foundProductTag);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.NoContent();
		}

		[HttpPut]
		[Route("{product_id:guid}/image_url")]
		public async Task<IActionResult> AddImageUrl([FromRoute(Name = "product_id")] Guid productId, [FromQuery(Name = "image_url")] Uri imageUrl)
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
			_bagsContext.ProductImageUrls.Add(new Models.ProductImageUrl { Product = foundProduct, Url = imageUrl.ToString() });
			await _bagsContext.SaveChangesAsync();

			return Ok(foundProduct.ToUnsafeExpandedWireFormat());
		}

		[HttpPut]
		[Route("{product_id:guid}/purchase_url")]
		public async Task<IActionResult> AddPurchaseUrl([FromRoute(Name = "product_id")] Guid productId, [FromQuery(Name = "purchase_url")] Uri purchaseUrl)
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
			_bagsContext.ProductPurchaseUrls.Add(new Models.ProductPurchaseUrl { Product = foundProduct, Url = purchaseUrl.ToString() });
			await _bagsContext.SaveChangesAsync();

			return Ok(foundProduct.ToUnsafeExpandedWireFormat());
		}
	}
}
