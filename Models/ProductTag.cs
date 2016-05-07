﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Data.Entity;

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

	public static class  ProductTagExtensions
	{
		public static IQueryable<ProductTag> WithTagIncludes(this IQueryable<ProductTag> query)
		{
			return query
				.Include(productTag => productTag.Tag).ThenInclude(tag => tag.TagCategory);
		}
	}
}
