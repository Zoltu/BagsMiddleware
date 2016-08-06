using Microsoft.EntityFrameworkCore;

namespace Zoltu.Bags.Api.Models
{
    public class BagsContext : DbContext
	{
		public DbSet<Product> Products { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<TagCategory> TagCategories { get; set; }
		public DbSet<ProductTag> ProductTags { get; set; }
		public DbSet<AmazonProduct> AmazonProducts { get; set; }

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

			// index that includes the ID (recommended by Azure); the advantage of this is that the index will contain all of the columns necessary for some queries so the table won't have to be queried at all
			modelBuilder.Entity<ProductTag>()
				.HasIndex(productTag => new { productTag.ProductId, productTag.Id });

			modelBuilder.Entity<Product>()
				.HasIndex(product => product.Asin)
				.IsUnique(true);

			// unique constraint on ASIN
			modelBuilder.Entity<AmazonProduct>()
				.HasAlternateKey(amazonProduct => amazonProduct.Asin);
		}
	}
}
