using Microsoft.AspNet.Mvc;

namespace Templates.ApiApp.Controllers
{
	[Route("api/[controller]")]
	public class TagsController : Controller
	{
		// GET: api/values
		[HttpGet]
		public IActionResult Get()
		{
			return Ok("Greetings.");
		}
	}
}
