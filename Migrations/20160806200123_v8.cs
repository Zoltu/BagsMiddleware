using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Zoltu.Bags.Api.Migrations
{
	public partial class v8 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "AmazonProducts",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					Asin = table.Column<string>(nullable: false),
					Available = table.Column<bool>(nullable: false),
					LastChecked = table.Column<DateTimeOffset>(nullable: false),
					Price = table.Column<int>(nullable: false),
					ProductId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AmazonProducts", x => x.Id);
					table.UniqueConstraint("AK_AmazonProducts_Asin", x => x.Asin);
					table.ForeignKey(
						name: "FK_AmazonProducts_Products_ProductId",
						column: x => x.ProductId,
						principalTable: "Products",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_AmazonProducts_ProductId",
				table: "AmazonProducts",
				column: "ProductId",
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AmazonProducts");
		}
	}
}
