using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Zoltu.BagsMiddleware.Extensions;
using Zoltu.BagsMiddleware.Models;

namespace Zoltu.BagsMiddleware.Controllers
{
	[Route("api/tags")]
	public class TagsController : Controller
	{
		private BagsContext _bagsContext;

		public TagsController(BagsContext bagsContext)
		{
			_bagsContext = bagsContext;
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> GetTags()
		{
			return HttpResult.Ok(await _bagsContext.Tags
				.WithSafeIncludes()
				.AsAsyncEnumerable()
				.Select(tag => tag.ToSafeExpandedWireFormat())
				.ToList());
		}

		[HttpGet]
		[Route("{tag_id:guid}")]
		public async Task<IActionResult> GetTag([FromRoute(Name = "tag_id")] Guid tagId)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// locate tag
			var foundTag = await _bagsContext.Tags
				.WithUnsafeIncludes()
				.Where(tag => tag.Id == tagId)
				.SingleOrDefaultAsync();
			if (foundTag == null)
				return HttpResult.NotFound();

			return HttpResult.Ok(foundTag.ToUnsafeExpandedWireFormat());
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateTag([FromQuery(Name = "category_id")] Guid categoryId, [FromQuery(Name = "name")] String name)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// verify category exists
			var category = await _bagsContext
				.TagCategories
				.Where(x => x.Id == categoryId)
				.SingleOrDefaultAsync();
			if (category == null)
				return HttpResult.BadRequest($"Category {categoryId} does not exist.");

			// check for existing tag
			var existingTag = await _bagsContext.Tags
				.WithSafeIncludes()
				.Where(tag => tag.Name == name && tag.TagCategoryId == categoryId)
				.SingleOrDefaultAsync();
			if (existingTag != null)
				return HttpResult.Ok(existingTag.ToSafeExpandedWireFormat());

			// create a new tag
			var newTag = new Models.Tag { Name = name, TagCategory = category };
			_bagsContext.Tags.Add(newTag);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(newTag.ToSafeExpandedWireFormat());
		}

		[HttpPut]
		[Route("{tag_id:guid}")]
		public async Task<IActionResult> EditTag([FromRoute(Name = "tag_id")] Guid tagId, [FromQuery(Name = "category_id")] Guid categoryId, [FromQuery(Name = "name")] String name)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// locate tag
			var foundTag = await _bagsContext.Tags
				.WithSafeIncludes()
				.Where(tag => tag.Id == tagId)
				.SingleOrDefaultAsync();
			if (foundTag == null)
				return HttpResult.NotFound();

			// verify there are changes
			if (foundTag.TagCategoryId == categoryId && foundTag.Name == name)
				return HttpResult.Ok(foundTag.ToSafeExpandedWireFormat());

			// locate the new category
			var newCategory = await _bagsContext.TagCategories
				.Where(category => category.Id == categoryId)
				.SingleOrDefaultAsync();
			if (newCategory == null)
				return HttpResult.BadRequest($"Category {categoryId} does not exist.");

			// verify no conflict on unique constraint
			var duplicateTag = await _bagsContext.Tags
				.WithSafeIncludes()
				.Where(tag => tag.TagCategoryId == categoryId && tag.Name == name)
				.SingleOrDefaultAsync();
			if (duplicateTag != null)
				return HttpResult.Conflict(duplicateTag.ToSafeExpandedWireFormat());

			// change the category/name
			foundTag.TagCategory = newCategory;
			foundTag.Name = name;
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(foundTag.ToSafeExpandedWireFormat());
		}

		[HttpDelete]
		[Route("{tag_id:guid}")]
		public async Task<IActionResult> DeleteTag([FromRoute(Name = "tag_id")] Guid tagId)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// locate tag
			var foundTag = await _bagsContext.Tags
				.Include(tag => tag.Products)
				.Where(tag => tag.Id == tagId)
				.SingleOrDefaultAsync();
			if (foundTag == null)
				return HttpResult.NoContent();

			// validate it doesn't have any products
			if (foundTag.Products.Any())
				return HttpResult.Conflict("Tag must have no associated products before it can be deleted.");

			// delete
			_bagsContext.Tags.Remove(foundTag);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.NoContent();
		}
	}
}
