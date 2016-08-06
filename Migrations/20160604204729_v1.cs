using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Zoltu.Bags.Api.Migrations
{
	public partial class v1 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Products",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					Name = table.Column<string>(nullable: false),
					Price = table.Column<long>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Products", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "TagCategories",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					Name = table.Column<string>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TagCategories", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "ProductImageUrls",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					ProductId = table.Column<Guid>(nullable: false),
					Url = table.Column<string>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ProductImageUrls", x => x.Id);
					table.ForeignKey(
						name: "FK_ProductImageUrls_Products_ProductId",
						column: x => x.ProductId,
						principalTable: "Products",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ProductPurchaseUrls",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					ProductId = table.Column<Guid>(nullable: false),
					Url = table.Column<string>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ProductPurchaseUrls", x => x.Id);
					table.ForeignKey(
						name: "FK_ProductPurchaseUrls_Products_ProductId",
						column: x => x.ProductId,
						principalTable: "Products",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Tags",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					Name = table.Column<string>(nullable: false),
					TagCategoryId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tags", x => x.Id);
					table.ForeignKey(
						name: "FK_Tags_TagCategories_TagCategoryId",
						column: x => x.TagCategoryId,
						principalTable: "TagCategories",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ProductTags",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					ProductId = table.Column<Guid>(nullable: false),
					TagId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ProductTags", x => x.Id);
					table.ForeignKey(
						name: "FK_ProductTags_Products_ProductId",
						column: x => x.ProductId,
						principalTable: "Products",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_ProductTags_Tags_TagId",
						column: x => x.TagId,
						principalTable: "Tags",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_ProductImageUrls_ProductId",
				table: "ProductImageUrls",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_ProductPurchaseUrls_ProductId",
				table: "ProductPurchaseUrls",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_ProductTags_ProductId",
				table: "ProductTags",
				column: "ProductId");

			migrationBuilder.CreateIndex(
				name: "IX_ProductTags_TagId",
				table: "ProductTags",
				column: "TagId");

			migrationBuilder.CreateIndex(
				name: "IX_ProductTags_TagId_ProductId",
				table: "ProductTags",
				columns: new[] { "TagId", "ProductId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Tags_TagCategoryId",
				table: "Tags",
				column: "TagCategoryId");

			migrationBuilder.CreateIndex(
				name: "IX_Tags_Name_TagCategoryId",
				table: "Tags",
				columns: new[] { "Name", "TagCategoryId" },
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_TagCategories_Name",
				table: "TagCategories",
				column: "Name",
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "ProductImageUrls");

			migrationBuilder.DropTable(
				name: "ProductPurchaseUrls");

			migrationBuilder.DropTable(
				name: "ProductTags");

			migrationBuilder.DropTable(
				name: "Products");

			migrationBuilder.DropTable(
				name: "Tags");

			migrationBuilder.DropTable(
				name: "TagCategories");
		}
	}
}
