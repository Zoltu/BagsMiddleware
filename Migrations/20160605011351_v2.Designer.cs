using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Zoltu.Bags.Api.Models;

namespace Zoltu.Bags.Api.Migrations
{
	[DbContext(typeof(BagsContext))]
	[Migration("20160605011351_v2")]
	partial class v2
	{
		protected override void BuildTargetModel(ModelBuilder modelBuilder)
		{
			modelBuilder
				.HasAnnotation("ProductVersion", "1.0.0-rc2-20901")
				.HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

			modelBuilder.Entity("Zoltu.Bags.Api.Models.Product", b =>
			{
				b.Property<int>("Id")
					.ValueGeneratedOnAdd();

				b.Property<string>("Name")
					.IsRequired();

				b.Property<long>("Price");

				b.HasKey("Id");

				b.ToTable("Products");
			});

			modelBuilder.Entity("Zoltu.Bags.Api.Models.ProductImageUrl", b =>
			{
				b.Property<Guid>("Id")
					.ValueGeneratedOnAdd();

				b.Property<int>("ProductId");

				b.Property<string>("Url")
					.IsRequired();

				b.HasKey("Id");

				b.HasIndex("ProductId");

				b.ToTable("ProductImageUrls");
			});

			modelBuilder.Entity("Zoltu.Bags.Api.Models.ProductPurchaseUrl", b =>
			{
				b.Property<Guid>("Id")
					.ValueGeneratedOnAdd();

				b.Property<int>("ProductId");

				b.Property<string>("Url")
					.IsRequired();

				b.HasKey("Id");

				b.HasIndex("ProductId");

				b.ToTable("ProductPurchaseUrls");
			});

			modelBuilder.Entity("Zoltu.Bags.Api.Models.ProductTag", b =>
			{
				b.Property<Guid>("Id")
					.ValueGeneratedOnAdd();

				b.Property<int>("ProductId");

				b.Property<Guid>("TagId");

				b.HasKey("Id");

				b.HasIndex("ProductId");

				b.HasIndex("TagId");

				b.HasIndex("TagId", "ProductId")
					.IsUnique();

				b.ToTable("ProductTags");
			});

			modelBuilder.Entity("Zoltu.Bags.Api.Models.Tag", b =>
			{
				b.Property<Guid>("Id")
					.ValueGeneratedOnAdd();

				b.Property<string>("Name")
					.IsRequired();

				b.Property<Guid>("TagCategoryId");

				b.HasKey("Id");

				b.HasIndex("TagCategoryId");

				b.HasIndex("Name", "TagCategoryId")
					.IsUnique();

				b.ToTable("Tags");
			});

			modelBuilder.Entity("Zoltu.Bags.Api.Models.TagCategory", b =>
			{
				b.Property<Guid>("Id")
					.ValueGeneratedOnAdd();

				b.Property<string>("Name")
					.IsRequired();

				b.HasKey("Id");

				b.HasIndex("Name")
					.IsUnique();

				b.ToTable("TagCategories");
			});

			modelBuilder.Entity("Zoltu.Bags.Api.Models.ProductImageUrl", b =>
			{
				b.HasOne("Zoltu.Bags.Api.Models.Product")
					.WithMany()
					.HasForeignKey("ProductId")
					.OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity("Zoltu.Bags.Api.Models.ProductPurchaseUrl", b =>
			{
				b.HasOne("Zoltu.Bags.Api.Models.Product")
					.WithMany()
					.HasForeignKey("ProductId")
					.OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity("Zoltu.Bags.Api.Models.ProductTag", b =>
			{
				b.HasOne("Zoltu.Bags.Api.Models.Product")
					.WithMany()
					.HasForeignKey("ProductId")
					.OnDelete(DeleteBehavior.Cascade);

				b.HasOne("Zoltu.Bags.Api.Models.Tag")
					.WithMany()
					.HasForeignKey("TagId")
					.OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity("Zoltu.Bags.Api.Models.Tag", b =>
			{
				b.HasOne("Zoltu.Bags.Api.Models.TagCategory")
					.WithMany()
					.HasForeignKey("TagCategoryId")
					.OnDelete(DeleteBehavior.Cascade);
			});
		}
	}
}
