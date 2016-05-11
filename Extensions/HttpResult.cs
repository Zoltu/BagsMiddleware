using System;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Zoltu.BagsMiddleware.Extensions
{
	public static class HttpResult
	{
		public static IActionResult Ok() { return new HttpStatusCodeResult(StatusCodes.Status200OK); }
		public static IActionResult Ok(Object value) { return new ObjectResult(value) { StatusCode = StatusCodes.Status200OK }; }

		public static IActionResult NoContent() { return new HttpStatusCodeResult(StatusCodes.Status204NoContent); }

		public static IActionResult BadRequest() { return new HttpStatusCodeResult(StatusCodes.Status400BadRequest); }
		public static IActionResult BadRequest(Object value) { return new ObjectResult(value) { StatusCode = StatusCodes.Status400BadRequest }; }
		public static IActionResult BadRequest(ModelStateDictionary modelState)
		{
			if (modelState == null) new ArgumentNullException(nameof(modelState));

			var error = modelState.SelectMany(x => x.Value.Errors).First();
			if (error?.ErrorMessage != null && error.ErrorMessage != String.Empty)
				return BadRequest(error.ErrorMessage);
			else if (error?.Exception?.Message != null)
				return BadRequest(error.Exception.Message);
			else
				return BadRequest(new SerializableError(modelState));
		}

		public static IActionResult NotFound() { return new HttpStatusCodeResult(StatusCodes.Status404NotFound); }
		public static IActionResult NotFound(Object value) { return new ObjectResult(value) { StatusCode = StatusCodes.Status404NotFound }; }

		public static IActionResult Conflict() { return new HttpStatusCodeResult(StatusCodes.Status409Conflict); }
		public static IActionResult Conflict(Object value) { return new ObjectResult(value) { StatusCode = StatusCodes.Status409Conflict }; }

		public static IActionResult InternalServerError() { return new HttpStatusCodeResult(StatusCodes.Status500InternalServerError); }
		public static IActionResult InternalServerError(Object value) { return new ObjectResult(value) { StatusCode = StatusCodes.Status500InternalServerError }; }
	}
}
