﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Zoltu.BagsMiddleware.Models
{
	public class ProductTag
    {
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid TagId { get; set; }
		public Tag Tag { get; set; }
		[Required]
		public Guid ProductId { get; set; }
		public Product Product { get; set; }
    }
}