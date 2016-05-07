using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;

namespace Zoltu.BagsMiddleware.Models
{
	public class TagCategory
    {
		[Key]
		public Guid Id { get; set; }

		[Required]
		public String Name { get; set; }

		public List<Tag> Tags { get; set; } = new List<Tag>();

		public dynamic ToBaseWireFormat()
		{
			dynamic result = new ExpandoObject();
			result.id = Id;
			result.name = Name;
			return result;
		}

		public dynamic ToExpandedWireFormat()
		{
			var result = ToBaseWireFormat();
			result.tags = Tags.Select(tag => tag.ToExpandedWireFormat()).ToList();
			return result;
		}
    }
}
