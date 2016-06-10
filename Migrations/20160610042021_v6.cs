using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BagsMiddleware.Migrations
{
    public partial class v6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Asin",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Asin",
                table: "Products",
                column: "Asin",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Asin",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Asin",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false);
        }
    }
}
