using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;

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

		public dynamic ToExpandedWireFormat()
		{
			var result = ToBaseWireFormat();
			result.image_urls = ImageUrls.Select(imageUrl => imageUrl.Url);
			result.purchase_urls = PurchaseUrls.Select(purchaseUrl => purchaseUrl.Url);
			result.tags = Tags.Select(productTag => productTag.Tag).Select(tag => tag.ToBaseWireFormat());
			return result;
		}
	}
}
