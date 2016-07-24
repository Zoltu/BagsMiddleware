using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Zoltu.BagsMiddleware.Models;

namespace BagsMiddleware.Migrations
{
    [DbContext(typeof(BagsContext))]
    [Migration("20160724045034_v7")]
    partial class v7
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Asin")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 450);

                    b.Property<string>("ImagesJson")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<long>("Price");

                    b.HasKey("Id");

                    b.HasIndex("Asin")
                        .IsUnique();

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.ProductTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ProductId");

                    b.Property<Guid>("TagId");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("TagId");

                    b.HasIndex("ProductId", "Id");

                    b.HasIndex("TagId", "ProductId")
                        .IsUnique();

                    b.ToTable("ProductTags");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.Tag", b =>
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

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.TagCategory", b =>
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

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.ProductTag", b =>
                {
                    b.HasOne("Zoltu.BagsMiddleware.Models.Product", "Product")
                        .WithMany("Tags")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Zoltu.BagsMiddleware.Models.Tag", "Tag")
                        .WithMany("Products")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.Tag", b =>
                {
                    b.HasOne("Zoltu.BagsMiddleware.Models.TagCategory", "TagCategory")
                        .WithMany("Tags")
                        .HasForeignKey("TagCategoryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
