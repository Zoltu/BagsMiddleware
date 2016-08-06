using Microsoft.EntityFrameworkCore.Migrations;

namespace Zoltu.Bags.Api.Migrations
{
	public partial class v4 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "ProductGuidId",
				table: "ProductTags");
			migrationBuilder.DropColumn(
				name: "ProductGuidId",
				table: "ProductPurchaseUrls");
			migrationBuilder.DropColumn(
				name: "GuidId",
				table: "Products");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
