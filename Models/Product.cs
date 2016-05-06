using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Zoltu.BagsMiddleware.Models
{
	public class Product
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public String Name { get; set; }
		[Required]
		public Int64 Price { get; set; }

		public List<ProductImageUrl> ImageUrls { get; set; } = new List<ProductImageUrl>();
		public List<ProductPurchaseUrl> PurchaseUrls { get; set; } = new List<ProductPurchaseUrl>();
		public List<ProductTag> Tags { get; set; } = new List<ProductTag>();
	}
}
