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
				.Select(category => new { id = category.Id, name = category.Name, tags = category.Tags.Select(tag => new { id = tag.Id, name = tag.Name }) })
				.ToListAsync());
		}

		[HttpPut]
		[Route("")]
		public async Task<IActionResult> CreateCategory([FromQuery(Name = "name")] String name)
		{
			if (!ModelState.IsValid)
				return HttpBadRequest();

			_bagsContext.TagCategories.Add(new Models.TagCategory { Name = name });
			await _bagsContext.SaveChangesAsync();

			return new NoContentResult();
		}
	}
}
