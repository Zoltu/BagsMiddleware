﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using Microsoft.Data.Entity;

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

		public dynamic ToBaseWireFormat()
		{
			dynamic result = new ExpandoObject();
			result.id = Id;
			result.name = Name;
			result.price = Price;
			return result;
		}

		public dynamic ToSafeExpandedWireFormat()
		{
			var result = ToBaseWireFormat();
			result.image_urls = ImageUrls.Select(imageUrl => imageUrl.Url).ToList();
			result.purchase_urls = PurchaseUrls.Select(purchaseUrl => purchaseUrl.Url).ToList();
			return result;
		}

		public dynamic ToUnsafeExpandedWireFormat()
		{
			var result = ToSafeExpandedWireFormat();
			result.tags = Tags.Select(productTag => productTag.Tag).Select(tag => tag.ToSafeExpandedWireFormat()).ToList();
			return result;
		}
	}

	public static class ProductExtensions
	{
		public static IQueryable<Product> WithSafeIncludes(this IQueryable<Product> query)
		{
			return query
				.Include(product => product.ImageUrls)
				.Include(product => product.PurchaseUrls);
		}

		public static IQueryable<Product> WithUnsafeIncludes(this IQueryable<Product> query)
		{
			return query
				.WithSafeIncludes()
				.Include(product => product.Tags).ThenInclude(productTag => productTag.Tag.TagCategory);
		}
	}
}
