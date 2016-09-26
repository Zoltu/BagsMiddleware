using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Zoltu.Bags.Api.Migrations
{
	public partial class v9 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				table: "Tags",
				name: "Id_x",
				nullable: false)
				.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
			migrationBuilder.AddColumn<int>(
				table: "ProductTags",
				name: "TagId_x",
				nullable: true);
			migrationBuilder.Sql(@"
update p
set TagId_x = t.Id_x
from ProductTags p
left join tags t on p.TagId = t.Id");
			migrationBuilder.AlterColumn<int>(
				table: "ProductTags",
				name: "TagId_x",
				nullable: false);
			migrationBuilder.DropIndex(
				table: "ProductTags",
				name: "IX_ProductTags_TagId_ProductId");
			migrationBuilder.DropIndex(
				table: "ProductTags",
				name: "IX_ProductTags_TagId");
			migrationBuilder.DropForeignKey(
				table: "ProductTags",
				name: "FK_ProductTags_Tags_TagId");
			migrationBuilder.DropColumn(
				table: "ProductTags",
				name: "TagId");
			migrationBuilder.DropPrimaryKey(
				table: "Tags",
				name: "PK_Tags");
			migrationBuilder.DropColumn(
				table: "Tags",
				name: "Id");
			migrationBuilder.RenameColumn(
				table: "Tags",
				name: "Id_x",
				newName: "Id");
			migrationBuilder.RenameColumn(
				table: "ProductTags",
				name: "TagId_x",
				newName: "TagId");
			migrationBuilder.AddPrimaryKey(
				table: "Tags",
				name: "PK_Tags",
				column: "Id");
			migrationBuilder.AddForeignKey(
				table: "ProductTags",
				name: "FK_ProductTags_Tags_TagId",
				column: "TagId",
				principalTable: "Tags",
				principalColumn: "Id");
			migrationBuilder.CreateIndex(
				table: "ProductTags",
				name: "IX_ProductTags_TagId",
				column: "TagId");
			migrationBuilder.CreateIndex(
				table: "ProductTags",
				name: "IX_ProductTags_TagId_ProductId",
				columns: new[] { "TagId", "ProductId" },
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
		}
	}
}
