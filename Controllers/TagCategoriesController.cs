using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Newtonsoft.Json;
using Zoltu.BagsMiddleware.Extensions;
using Zoltu.BagsMiddleware.Models;

namespace Zoltu.BagsMiddleware.Controllers
{
	[Route("api/tag_categories")]
	public class TagCategoriesController : Controller
	{
		private Models.BagsContext _bagsContext;

		public TagCategoriesController(Models.BagsContext bagsContext)
		{
			_bagsContext = bagsContext;
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> GetCategories()
		{
			return HttpResult.Ok(await _bagsContext.TagCategories
				.AsAsyncEnumerable()
				.Select(category => category.ToBaseWireFormat())
				.ToList());
		}

		[HttpGet]
		[Route("{category_id:guid}")]
		public async Task<IActionResult> GetCategory([FromRoute(Name = "category_id")] Guid categoryId)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// get category
			var foundTagCategory = await _bagsContext.TagCategories
				.WithUnsafeIncludes()
				.Where(category => category.Id == categoryId)
				.SingleOrDefaultAsync();
			if (foundTagCategory == null)
				return HttpResult.NotFound();

			return HttpResult.Ok(foundTagCategory.ToUnsafeExpandedWireFormat());
		}

		public class CreateCategoryRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "name")]
			public String Name { get; set; }
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// check for existing category
			var foundTagCategory = await _bagsContext.TagCategories
				.Where(category => category.Name == request.Name)
				.SingleOrDefaultAsync();
			if (foundTagCategory != null)
				return HttpResult.Ok(foundTagCategory.ToBaseWireFormat());

			// create a new tag
			var newTagCategory = new Models.TagCategory { Name = request.Name };
			_bagsContext.TagCategories.Add(newTagCategory);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(newTagCategory.ToBaseWireFormat());
		}

		public class EditCategoryRequest
		{
			[JsonProperty(Required = Required.Always, PropertyName = "name")]
			public String Name { get; set; }
		}

		[HttpPut]
		[Route("{category_id:guid}")]
		public async Task<IActionResult> EditCategory([FromRoute(Name = "category_id")] Guid categoryId, [FromBody] EditCategoryRequest request)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// locate category
			var foundCategory = await _bagsContext.TagCategories
				.Where(category => category.Id == categoryId)
				.SingleOrDefaultAsync();
			if (foundCategory == null)
				return HttpResult.NotFound();

			// verify there are actual changes
			if (foundCategory.Name == request.Name)
				return HttpResult.Ok(foundCategory.ToBaseWireFormat());

			// verify no conflicts with unique constraint
			var duplicateCategory = await _bagsContext.TagCategories
				.Where(category => category.Name == request.Name)
				.SingleOrDefaultAsync();
			if (duplicateCategory != null)
				return HttpResult.Conflict(duplicateCategory.ToBaseWireFormat());

			// change the name
			foundCategory.Name = request.Name;
			await _bagsContext.SaveChangesAsync();

			return HttpResult.Ok(foundCategory.ToBaseWireFormat());
		}

		[HttpDelete]
		[Route("{category_id:guid}")]
		public async Task<IActionResult> DeleteCategory([FromRoute(Name = "category_id")] Guid categoryId)
		{
			// validate input
			if (!ModelState.IsValid)
				return HttpResult.BadRequest(ModelState);

			// locate category
			var foundCategory = await _bagsContext.TagCategories
				.Where(category => category.Id == categoryId)
				.SingleOrDefaultAsync();
			if (foundCategory == null)
				return HttpResult.NoContent();

			// validate it doesn't have any tags
			if (foundCategory.Tags.Any())
				return HttpResult.Conflict("Category must have no associated tags before it can be deleted.");

			// delete
			_bagsContext.TagCategories.Remove(foundCategory);
			await _bagsContext.SaveChangesAsync();

			return HttpResult.NoContent();
		}
	}
}
