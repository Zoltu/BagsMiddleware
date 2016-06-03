using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
				.WithSafeIncludes()
				.Where(tag => tag.Id == tagId)
				.SingleOrDefaultAsync();
			if (foundTag == null)
				return HttpResult.NotFound();

			return HttpResult.Ok(foundTag.ToSafeExpandedWireFormat());
		}

		public class CreateTagRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "category_id")]
			public Guid CategoryId { get; set; }
			[JsonProperty(Required = Required.Always, PropertyName = "name")]
			public String Name { get; set; }
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// verify category exists
			var category = await _bagsContext
				.TagCategories
				.Where(x => x.Id == request.CategoryId)
				.SingleOrDefaultAsync();
			if (category == null)
				return HttpResult.BadRequest($"Category {request.CategoryId} does not exist.");

			// check for existing tag
			var existingTag = await _bagsContext.Tags
				.WithSafeIncludes()
				.Where(tag => tag.Name == request.Name && tag.TagCategoryId == request.CategoryId)
				.SingleOrDefaultAsync();
			if (existingTag != null)
				return HttpResult.Ok(existingTag.ToSafeExpandedWireFormat());

			// create a new tag
			var newTag = new Models.Tag { Name = request.Name, TagCategory = category };
			_bagsContext.Tags.Add(newTag);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(newTag.ToSafeExpandedWireFormat());
		}

		public class EditTagRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "category_id")]
			public Guid CategoryId { get; set; }

			[JsonProperty(Required = Required.Always, PropertyName = "name")]
			public String Name { get; set; }
		}

		[HttpPut]
		[Route("{tag_id:guid}")]
		public async Task<IActionResult> EditTag([FromRoute(Name = "tag_id")] Guid tagId, [FromBody] EditTagRequest request)
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
			if (foundTag.TagCategoryId == request.CategoryId && foundTag.Name == request.Name)
				return HttpResult.Ok(foundTag.ToSafeExpandedWireFormat());

			// locate the new category
			var newCategory = await _bagsContext.TagCategories
				.Where(category => category.Id == request.CategoryId)
				.SingleOrDefaultAsync();
			if (newCategory == null)
				return HttpResult.BadRequest($"Category {request.CategoryId} does not exist.");

			// verify no conflict on unique constraint
			var duplicateTag = await _bagsContext.Tags
				.WithSafeIncludes()
				.Where(tag => tag.TagCategoryId == request.CategoryId && tag.Name == request.Name)
				.SingleOrDefaultAsync();
			if (duplicateTag != null)
				return HttpResult.Conflict(duplicateTag.ToSafeExpandedWireFormat());

			// change the category/name
			foundTag.TagCategory = newCategory;
			foundTag.Name = request.Name;
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
				.Where(tag => tag.Id == tagId)
				.SingleOrDefaultAsync();
			if (foundTag == null)
				return HttpResult.NoContent();

			// validate it doesn't have any products
			var foundProducts = await _bagsContext.Products
				.Where(product => product.Tags.Contains(foundTag))
				.ToListAsync();
			if (foundProducts.Any())
				return HttpResult.Conflict("Tag must have no associated products before it can be deleted.");

			// delete
			_bagsContext.Tags.Remove(foundTag);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.NoContent();
		}
	}
}
