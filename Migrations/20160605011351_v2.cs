using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BagsMiddleware.Migrations
{
	public partial class v2 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// drop keys and indexes
			migrationBuilder.DropForeignKey(
				name: "FK_ProductImageUrls_Products_ProductId",
				table: "ProductImageUrls");
			migrationBuilder.DropForeignKey(
				name: "FK_ProductPurchaseUrls_Products_ProductId",
				table: "ProductPurchaseUrls");
			migrationBuilder.DropForeignKey(
				name: "FK_ProductTags_Products_ProductId",
				table: "ProductTags");
			migrationBuilder.DropIndex(
				name: "IX_ProductImageUrls_ProductId",
				table: "ProductImageUrls");
			migrationBuilder.DropIndex(
				name: "IX_ProductPurchaseUrls_ProductId",
				table: "ProductPurchaseUrls");
			migrationBuilder.DropIndex(
				name: "IX_ProductTags_ProductId",
				table: "ProductTags");
			migrationBuilder.DropIndex(
				name: "IX_ProductTags_TagId_ProductId",
				table: "ProductTags");
			migrationBuilder.DropPrimaryKey(
				name: "PK_Products",
				table: "Products");

			// rename columns to *GuidId
			migrationBuilder.RenameColumn(
				name: "Id",
				table: "Products",
				newName: "GuidId");
			migrationBuilder.RenameColumn(
				name: "ProductId",
				table: "ProductImageUrls",
				newName: "ProductGuidId");
			migrationBuilder.RenameColumn(
				name: "ProductId",
				table: "ProductPurchaseUrls",
				newName: "ProductGuidId");
			migrationBuilder.RenameColumn(
				name: "ProductId",
				table: "ProductTags",
				newName: "ProductGuidId");

			// create new columns
			migrationBuilder.AddColumn<int>(
				name: "Id",
				table: "Products",
				type: "int",
				nullable: false)
				.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
			migrationBuilder.AddPrimaryKey(
				name: "PK_Products",
				table: "Products",
				column: "Id");
			migrationBuilder.AddColumn<int>(
				name: "ProductId",
				table: "ProductImageUrls",
				type: "int",
				nullable: true);
			migrationBuilder.AddColumn<int>(
				name: "ProductId",
				table: "ProductPurchaseUrls",
				type: "int",
				nullable: true);
			migrationBuilder.AddColumn<int>(
				name: "ProductId",
				table: "ProductTags",
				type: "int",
				nullable: true);

			// migrate mappings to new keys
			migrationBuilder.Sql(@"
UPDATE urls
SET urls.ProductId = products.Id
FROM ProductPurchaseUrls urls
INNER JOIN Products products ON urls.ProductGuidId = products.GuidId");
			migrationBuilder.Sql(@"
UPDATE urls
SET urls.ProductId = products.Id
FROM ProductImageUrls urls
INNER JOIN Products products ON urls.ProductGuidId = products.GuidId");
			migrationBuilder.Sql(@"
UPDATE productTags
SET productTags.ProductId = products.Id
FROM ProductTags productTags
INNER JOIN Products products ON productTags.ProductGuidId = products.GuidId");

			// add constraints and indexes
			migrationBuilder.AlterColumn<int>(
				name: "ProductId",
				table: "ProductImageUrls",
				type: "int",
				nullable: false);
			migrationBuilder.AlterColumn<int>(
				name: "ProductId",
				table: "ProductPurchaseUrls",
				type: "int",
				nullable: false);
			migrationBuilder.AlterColumn<int>(
				name: "ProductId",
				table: "ProductTags",
				type: "int",
				nullable: false);
			migrationBuilder.AddForeignKey(
				name: "FK_ProductImageUrls_Products_ProductId",
				table: "ProductImageUrls",
				column: "ProductId",
				principalTable: "Products",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
			migrationBuilder.AddForeignKey(
				name: "FK_ProductPurchaseUrls_Products_ProductId",
				table: "ProductPurchaseUrls",
				column: "ProductId",
				principalTable: "Products",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
			migrationBuilder.AddForeignKey(
				name: "FK_ProductTags_Products_ProductId",
				table: "ProductTags",
				column: "ProductId",
				principalTable: "Products",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
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
				name: "IX_ProductTags_TagId_ProductId",
				table: "ProductTags",
				columns: new[] { "TagId", "ProductId" },
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
