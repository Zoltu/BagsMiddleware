using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

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
			return Ok(await _bagsContext.TagCategories
				.Select(category => category.ToExpandedWireFormat())
				.ToListAsync());
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateCategory([FromQuery(Name = "name")] String name)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest(ModelState);

			var newTagCategory = new Models.TagCategory { Name = name };
			_bagsContext.TagCategories.Add(newTagCategory);
			await _bagsContext.SaveChangesAsync();

			return Ok(newTagCategory.ToExpandedWireFormat());
		}
	}
}
