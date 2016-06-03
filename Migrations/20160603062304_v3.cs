using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BagsMiddleware.Migrations
{
    public partial class v3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Zoltu.BagsMiddleware.Models.ProductImageUrl_Zoltu.BagsMiddleware.Models.Product_ProductId",
                table: "Zoltu.BagsMiddleware.Models.ProductImageUrl");

            migrationBuilder.DropForeignKey(
                name: "FK_Zoltu.BagsMiddleware.Models.ProductPurchaseUrl_Zoltu.BagsMiddleware.Models.Product_ProductId",
                table: "Zoltu.BagsMiddleware.Models.ProductPurchaseUrl");

            migrationBuilder.DropForeignKey(
                name: "FK_Zoltu.BagsMiddleware.Models.Tag_Zoltu.BagsMiddleware.Models.TagCategory_TagCategoryId",
                table: "Zoltu.BagsMiddleware.Models.Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.TagCategory",
                table: "Zoltu.BagsMiddleware.Models.TagCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.Tag",
                table: "Zoltu.BagsMiddleware.Models.Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.ProductPurchaseUrl",
                table: "Zoltu.BagsMiddleware.Models.ProductPurchaseUrl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.ProductImageUrl",
                table: "Zoltu.BagsMiddleware.Models.ProductImageUrl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.Product",
                table: "Zoltu.BagsMiddleware.Models.Product");

            migrationBuilder.DropTable(
                name: "Zoltu.BagsMiddleware.Models.ProductTag");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TagCategory",
                table: "TagCategory",
                column: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Tag",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_ProductId",
                table: "Tag",
                column: "ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductPurchaseUrl",
                table: "ProductPurchaseUrl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImageUrl",
                table: "ProductImageUrl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImageUrl_Product_ProductId",
                table: "ProductImageUrl",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPurchaseUrl_Product_ProductId",
                table: "ProductPurchaseUrl",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Product_ProductId",
                table: "Tag",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_TagCategory_TagCategoryId",
                table: "Tag",
                column: "TagCategoryId",
                principalTable: "TagCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.RenameIndex(
                name: "IX_Zoltu.BagsMiddleware.Models.TagCategory_Name",
                table: "Zoltu.BagsMiddleware.Models.TagCategory",
                newName: "IX_TagCategory_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Zoltu.BagsMiddleware.Models.Tag_Name_TagCategoryId",
                table: "Zoltu.BagsMiddleware.Models.Tag",
                newName: "IX_Tag_Name_TagCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Zoltu.BagsMiddleware.Models.Tag_TagCategoryId",
                table: "Zoltu.BagsMiddleware.Models.Tag",
                newName: "IX_Tag_TagCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Zoltu.BagsMiddleware.Models.ProductPurchaseUrl_ProductId",
                table: "Zoltu.BagsMiddleware.Models.ProductPurchaseUrl",
                newName: "IX_ProductPurchaseUrl_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Zoltu.BagsMiddleware.Models.ProductImageUrl_ProductId",
                table: "Zoltu.BagsMiddleware.Models.ProductImageUrl",
                newName: "IX_ProductImageUrl_ProductId");

            migrationBuilder.RenameTable(
                name: "Zoltu.BagsMiddleware.Models.TagCategory",
                newName: "TagCategory");

            migrationBuilder.RenameTable(
                name: "Zoltu.BagsMiddleware.Models.Tag",
                newName: "Tag");

            migrationBuilder.RenameTable(
                name: "Zoltu.BagsMiddleware.Models.ProductPurchaseUrl",
                newName: "ProductPurchaseUrl");

            migrationBuilder.RenameTable(
                name: "Zoltu.BagsMiddleware.Models.ProductImageUrl",
                newName: "ProductImageUrl");

            migrationBuilder.RenameTable(
                name: "Zoltu.BagsMiddleware.Models.Product",
                newName: "Product");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImageUrl_Product_ProductId",
                table: "ProductImageUrl");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPurchaseUrl_Product_ProductId",
                table: "ProductPurchaseUrl");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Product_ProductId",
                table: "Tag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_TagCategory_TagCategoryId",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TagCategory",
                table: "TagCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_ProductId",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductPurchaseUrl",
                table: "ProductPurchaseUrl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImageUrl",
                table: "ProductImageUrl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Tag");

            migrationBuilder.CreateTable(
                name: "Zoltu.BagsMiddleware.Models.ProductTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    TagId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zoltu.BagsMiddleware.Models.ProductTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zoltu.BagsMiddleware.Models.ProductTag_Zoltu.BagsMiddleware.Models.Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Zoltu.BagsMiddleware.Models.Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Zoltu.BagsMiddleware.Models.ProductTag_Zoltu.BagsMiddleware.Models.Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Zoltu.BagsMiddleware.Models.Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.TagCategory",
                table: "Zoltu.BagsMiddleware.Models.TagCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.Tag",
                table: "Zoltu.BagsMiddleware.Models.Tag",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.ProductPurchaseUrl",
                table: "Zoltu.BagsMiddleware.Models.ProductPurchaseUrl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.ProductImageUrl",
                table: "Zoltu.BagsMiddleware.Models.ProductImageUrl",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Zoltu.BagsMiddleware.Models.Product",
                table: "Zoltu.BagsMiddleware.Models.Product",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Zoltu.BagsMiddleware.Models.ProductTag_ProductId",
                table: "Zoltu.BagsMiddleware.Models.ProductTag",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Zoltu.BagsMiddleware.Models.ProductTag_TagId",
                table: "Zoltu.BagsMiddleware.Models.ProductTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Zoltu.BagsMiddleware.Models.ProductTag_TagId_ProductId",
                table: "Zoltu.BagsMiddleware.Models.ProductTag",
                columns: new[] { "TagId", "ProductId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Zoltu.BagsMiddleware.Models.ProductImageUrl_Zoltu.BagsMiddleware.Models.Product_ProductId",
                table: "Zoltu.BagsMiddleware.Models.ProductImageUrl",
                column: "ProductId",
                principalTable: "Zoltu.BagsMiddleware.Models.Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zoltu.BagsMiddleware.Models.ProductPurchaseUrl_Zoltu.BagsMiddleware.Models.Product_ProductId",
                table: "Zoltu.BagsMiddleware.Models.ProductPurchaseUrl",
                column: "ProductId",
                principalTable: "Zoltu.BagsMiddleware.Models.Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zoltu.BagsMiddleware.Models.Tag_Zoltu.BagsMiddleware.Models.TagCategory_TagCategoryId",
                table: "Zoltu.BagsMiddleware.Models.Tag",
                column: "TagCategoryId",
                principalTable: "Zoltu.BagsMiddleware.Models.TagCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.RenameIndex(
                name: "IX_TagCategory_Name",
                table: "TagCategory",
                newName: "IX_Zoltu.BagsMiddleware.Models.TagCategory_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_Name_TagCategoryId",
                table: "Tag",
                newName: "IX_Zoltu.BagsMiddleware.Models.Tag_Name_TagCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_TagCategoryId",
                table: "Tag",
                newName: "IX_Zoltu.BagsMiddleware.Models.Tag_TagCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPurchaseUrl_ProductId",
                table: "ProductPurchaseUrl",
                newName: "IX_Zoltu.BagsMiddleware.Models.ProductPurchaseUrl_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImageUrl_ProductId",
                table: "ProductImageUrl",
                newName: "IX_Zoltu.BagsMiddleware.Models.ProductImageUrl_ProductId");

            migrationBuilder.RenameTable(
                name: "TagCategory",
                newName: "Zoltu.BagsMiddleware.Models.TagCategory");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Zoltu.BagsMiddleware.Models.Tag");

            migrationBuilder.RenameTable(
                name: "ProductPurchaseUrl",
                newName: "Zoltu.BagsMiddleware.Models.ProductPurchaseUrl");

            migrationBuilder.RenameTable(
                name: "ProductImageUrl",
                newName: "Zoltu.BagsMiddleware.Models.ProductImageUrl");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Zoltu.BagsMiddleware.Models.Product");
        }
    }
}
