﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Zoltu.BagsMiddleware.Models
{
	public class ProductImageUrl
    {
		[Key]
		public Guid Id { get; set; }

		[Required]
		public String Url { get; set; }
		[Required]
		public Int32 ProductId { get; set; }
		public Product Product { get; set; }
	}
}
