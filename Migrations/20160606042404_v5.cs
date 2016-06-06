using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BagsMiddleware.Migrations
{
	public partial class v5 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "ProductPurchaseUrls");
			migrationBuilder.AlterColumn<string>(
				name: "Asin",
				table: "Products",
				nullable: false);
			migrationBuilder.AlterColumn<string>(
				name: "ImagesJson",
				table: "Products",
				nullable: false);

		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "ProductPurchaseUrls",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					ProductId = table.Column<int>(nullable: false),
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

			migrationBuilder.CreateIndex(
				name: "IX_ProductPurchaseUrls_ProductId",
				table: "ProductPurchaseUrls",
				column: "ProductId");
		}
	}
}
