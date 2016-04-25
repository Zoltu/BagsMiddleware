using System;
using System.ComponentModel.DataAnnotations;

namespace Zoltu.BagsMiddleware.Models
{
	public class ProductPurchaseUrl
    {
		[Key]
		public Guid Id { get; set; }

		[Required]
		public String Url { get; set; }
		[Required]
		public Guid ProductId { get; set; }
		public Product Product { get; set; }
	}
}
