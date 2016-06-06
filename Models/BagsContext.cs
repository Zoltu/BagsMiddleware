using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Zoltu.BagsMiddleware.Models
{
	public class BagsContext : DbContext
    {
		public DbSet<Product> Products { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<TagCategory> TagCategories { get; set; }
		public DbSet<ProductTag> ProductTags { get; set; }

		public BagsContext(DbContextOptions<BagsContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// unique constraint on tag name & category
			modelBuilder.Entity<Tag>()
				.HasIndex(tag => new { tag.Name, tag.TagCategoryId })
				.IsUnique(true);

			// unique constraint on category name
			modelBuilder.Entity<TagCategory>()
				.HasIndex(category => category.Name)
				.IsUnique(true);

			// unique constraint on product tag
			modelBuilder.Entity<ProductTag>()
				.HasIndex(productTag => new { productTag.TagId, productTag.ProductId })
				.IsUnique(true);
		}
	}
}
