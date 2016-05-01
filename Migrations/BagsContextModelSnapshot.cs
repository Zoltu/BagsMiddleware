using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Zoltu.BagsMiddleware.Models;

namespace BagsMiddleware.Migrations
{
    [DbContext(typeof(BagsContext))]
    partial class BagsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-final");

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("Height");

                    b.Property<uint>("Length");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<uint>("Price");

                    b.Property<uint>("Width");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.ProductImageUrl", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ProductId");

                    b.Property<string>("Url")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.ProductPurchaseUrl", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ProductId");

                    b.Property<string>("Url")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.ProductTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ProductId");

                    b.Property<Guid>("TagId");

                    b.HasKey("Id");

                    b.HasAlternateKey("TagId", "ProductId");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid>("TagCategoryId");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name", "TagCategoryId");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.TagCategory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAlternateKey("Name");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.ProductImageUrl", b =>
                {
                    b.HasOne("Zoltu.BagsMiddleware.Models.Product")
                        .WithMany()
                        .HasForeignKey("ProductId");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.ProductPurchaseUrl", b =>
                {
                    b.HasOne("Zoltu.BagsMiddleware.Models.Product")
                        .WithMany()
                        .HasForeignKey("ProductId");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.ProductTag", b =>
                {
                    b.HasOne("Zoltu.BagsMiddleware.Models.Product")
                        .WithMany()
                        .HasForeignKey("ProductId");

                    b.HasOne("Zoltu.BagsMiddleware.Models.Tag")
                        .WithMany()
                        .HasForeignKey("TagId");
                });

            modelBuilder.Entity("Zoltu.BagsMiddleware.Models.Tag", b =>
                {
                    b.HasOne("Zoltu.BagsMiddleware.Models.TagCategory")
                        .WithMany()
                        .HasForeignKey("TagCategoryId");
                });
        }
    }
}
