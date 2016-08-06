using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Zoltu.Bags.Api.Migrations
{
	public partial class v3 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "ProductImageUrls");

			migrationBuilder.AddColumn<string>(
				name: "Asin",
				table: "Products",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "ImagesJson",
				table: "Products",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Asin",
				table: "Products");

			migrationBuilder.DropColumn(
				name: "ImagesJson",
				table: "Products");

			migrationBuilder.CreateTable(
				name: "ProductImageUrls",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					ProductId = table.Column<int>(nullable: false),
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

			migrationBuilder.CreateIndex(
				name: "IX_ProductImageUrls_ProductId",
				table: "ProductImageUrls",
				column: "ProductId");
		}
	}
}
