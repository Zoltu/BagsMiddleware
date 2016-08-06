using Microsoft.EntityFrameworkCore.Migrations;

namespace Zoltu.Bags.Api.Migrations
{
    public partial class v7 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
				name: "IX_ProductTags_ProductId_Id",
				table: "ProductTags",
				columns: new[] { "ProductId", "Id" });
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_ProductTags_ProductId_Id",
				table: "ProductTags");
		}
	}
}
