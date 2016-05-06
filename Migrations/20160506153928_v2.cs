using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace BagsMiddleware.Migrations
{
    public partial class v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ProductImageUrl_Product_ProductId", table: "ProductImageUrl");
            migrationBuilder.DropForeignKey(name: "FK_ProductPurchaseUrl_Product_ProductId", table: "ProductPurchaseUrl");
            migrationBuilder.DropForeignKey(name: "FK_ProductTag_Product_ProductId", table: "ProductTag");
            migrationBuilder.DropForeignKey(name: "FK_ProductTag_Tag_TagId", table: "ProductTag");
            migrationBuilder.DropForeignKey(name: "FK_Tag_TagCategory_TagCategoryId", table: "Tag");
            migrationBuilder.DropColumn(name: "Height", table: "Product");
            migrationBuilder.DropColumn(name: "Length", table: "Product");
            migrationBuilder.DropColumn(name: "Width", table: "Product");
            migrationBuilder.AlterColumn<long>(
                name: "Price",
                table: "Product",
                nullable: false);
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
                name: "FK_ProductTag_Product_ProductId",
                table: "ProductTag",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Tag_TagId",
                table: "ProductTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Tag_TagCategory_TagCategoryId",
                table: "Tag",
                column: "TagCategoryId",
                principalTable: "TagCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ProductImageUrl_Product_ProductId", table: "ProductImageUrl");
            migrationBuilder.DropForeignKey(name: "FK_ProductPurchaseUrl_Product_ProductId", table: "ProductPurchaseUrl");
            migrationBuilder.DropForeignKey(name: "FK_ProductTag_Product_ProductId", table: "ProductTag");
            migrationBuilder.DropForeignKey(name: "FK_ProductTag_Tag_TagId", table: "ProductTag");
            migrationBuilder.DropForeignKey(name: "FK_Tag_TagCategory_TagCategoryId", table: "Tag");
            migrationBuilder.AlterColumn<uint>(
                name: "Price",
                table: "Product",
                nullable: false);
            migrationBuilder.AddColumn<uint>(
                name: "Height",
                table: "Product",
                nullable: false,
                defaultValue: 0u);
            migrationBuilder.AddColumn<uint>(
                name: "Length",
                table: "Product",
                nullable: false,
                defaultValue: 0u);
            migrationBuilder.AddColumn<uint>(
                name: "Width",
                table: "Product",
                nullable: false,
                defaultValue: 0u);
            migrationBuilder.AddForeignKey(
                name: "FK_ProductImageUrl_Product_ProductId",
                table: "ProductImageUrl",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ProductPurchaseUrl_Product_ProductId",
                table: "ProductPurchaseUrl",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Product_ProductId",
                table: "ProductTag",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Tag_TagId",
                table: "ProductTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Tag_TagCategory_TagCategoryId",
                table: "Tag",
                column: "TagCategoryId",
                principalTable: "TagCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
