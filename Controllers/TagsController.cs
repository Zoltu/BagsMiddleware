using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace Zoltu.BagsMiddleware.Controllers
{
	[Route("api/tags")]
	public class TagsController : Controller
	{
		private Models.BagsContext _bagsContext;

		public TagsController(Models.BagsContext bagsContext)
		{
			_bagsContext = bagsContext;
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> GetTags()
		{
			return Ok(await _bagsContext.Tags
				.Select(tag => new { id = tag.Id, name = tag.Name, category_id = tag.TagCategoryId, category_name = tag.TagCategory.Name })
				.ToListAsync());
		}

		[HttpGet]
		[Route("by_category/{category_id:guid}")]
		public async Task<IActionResult> GetTagsByCategory([FromRoute(Name = "category_id")] Guid categoryId)
		{
			return Ok(await _bagsContext.Tags
				.Where(tag => tag.TagCategoryId == categoryId)
				.Select(tag => new { id = tag.Id, name = tag.Name, category_id = tag.TagCategoryId, category_name = tag.TagCategory.Name })
				.ToListAsync());
		}

		[HttpGet]
		[Route("by_product/{product_id:guid}")]
		public async Task<IActionResult> GetTagsByProduct([FromRoute(Name = "product_id")] Guid productId)
		{
			return Ok(await _bagsContext.ProductTags
				.Where(productTag => productTag.ProductId == productId)
				.Select(productTag => productTag.Tag)
				.Select(tag => new { id = tag.Id, name = tag.Name, category_id = tag.TagCategoryId, category_name = tag.TagCategory.Name })
				.ToListAsync());
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateTag([FromQuery(Name = "category_id")] Guid categoryId, [FromQuery(Name = "name")] String name)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest();

			var category = await _bagsContext.TagCategories.Where(x => x.Id == categoryId).SingleAsync();
			var newTag = new Models.Tag { Name = name, TagCategory = category };
			_bagsContext.Tags.Add(newTag);
			await _bagsContext.SaveChangesAsync();

			return new NoContentResult();
		}
	}
}
