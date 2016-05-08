using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using Microsoft.Data.Entity;

namespace Zoltu.BagsMiddleware.Models
{
	public class Tag
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public String Name { get; set; }

		[Required]
		public Guid TagCategoryId { get; set; }
		public TagCategory TagCategory { get; set; }

		public List<ProductTag> Products { get; set; } = new List<ProductTag>();

		public dynamic ToBaseWireFormat()
		{
			dynamic result = new ExpandoObject();
			result.id = Id;
			result.name = Name;
			result.category_id = TagCategoryId;
			return result;
		}

		public dynamic ToSafeExpandedWireFormat()
		{
			var result = ToBaseWireFormat();
			result.category = TagCategory.ToBaseWireFormat();
			return result;
		}

		public dynamic ToUnsafeExpandedWireFormat()
		{
			dynamic result = ToSafeExpandedWireFormat();
			result.products = Products.Select(product => product.Product.ToSafeExpandedWireFormat());
			return result;
		}
	}

	public static class TagExtensions
	{
		public static IQueryable<Tag> WithSafeIncludes(this IQueryable<Tag> query)
		{
			return query
				.Include(tag => tag.TagCategory);
		}

		public static IQueryable<Tag> WithUnsafeIncludes(this IQueryable<Tag> query)
		{
			return query
				.WithSafeIncludes()
				.Include(tag => tag.Products).ThenInclude(productTag => productTag.Product).ThenInclude(product => product.ImageUrls)
				.Include(tag => tag.Products).ThenInclude(productTag => productTag.Product).ThenInclude(product => product.PurchaseUrls)
				.Include(tag => tag.Products).ThenInclude(productTag => productTag.Product).ThenInclude(product => product.Tags).ThenInclude(productTag => productTag.Tag).ThenInclude(tag => tag.TagCategory);
		}
	}
}
