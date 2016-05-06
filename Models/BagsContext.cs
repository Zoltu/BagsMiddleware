using Microsoft.Data.Entity;

namespace Zoltu.BagsMiddleware.Models
{
	public class BagsContext : DbContext
    {
		public DbSet<Product> Products { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<TagCategory> TagCategories { get; set; }
		public DbSet<ProductTag> ProductTags { get; set; }
		public DbSet<ProductImageUrl> ProductImageUrls { get; set; }
		public DbSet<ProductPurchaseUrl> ProductPurchaseUrls { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// unique constraint on tag name & category
			modelBuilder.Entity<Tag>()
				.HasAlternateKey(tag => new { tag.Name, tag.TagCategoryId });

			// unique constraint on category name
			modelBuilder.Entity<TagCategory>()
				.HasAlternateKey(category => category.Name);

			// unique constraint on product tag
			modelBuilder.Entity<ProductTag>()
				.HasAlternateKey(productTag => new { productTag.TagId, productTag.ProductId });
		}
	}
}
