using Microsoft.Data.Entity.Migrations;

namespace BagsMiddleware.Migrations
{
	public partial class v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(name: "AK_TagCategory_Name", table: "TagCategory");
            migrationBuilder.DropUniqueConstraint(name: "AK_Tag_Name_TagCategoryId", table: "Tag");
            migrationBuilder.DropUniqueConstraint(name: "AK_ProductTag_TagId_ProductId", table: "ProductTag");
            migrationBuilder.CreateIndex(
                name: "IX_TagCategory_Name",
                table: "TagCategory",
                column: "Name",
                unique: true);
            migrationBuilder.CreateIndex(
                name: "IX_Tag_Name_TagCategoryId",
                table: "Tag",
                columns: new[] { "Name", "TagCategoryId" },
                unique: true);
            migrationBuilder.CreateIndex(
                name: "IX_ProductTag_TagId_ProductId",
                table: "ProductTag",
                columns: new[] { "TagId", "ProductId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_TagCategory_Name", table: "TagCategory");
            migrationBuilder.DropIndex(name: "IX_Tag_Name_TagCategoryId", table: "Tag");
            migrationBuilder.DropIndex(name: "IX_ProductTag_TagId_ProductId", table: "ProductTag");
            migrationBuilder.AddUniqueConstraint(
                name: "AK_TagCategory_Name",
                table: "TagCategory",
                column: "Name");
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Tag_Name_TagCategoryId",
                table: "Tag",
                columns: new[] { "Name", "TagCategoryId" });
            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProductTag_TagId_ProductId",
                table: "ProductTag",
                columns: new[] { "TagId", "ProductId" });
        }
    }
}
