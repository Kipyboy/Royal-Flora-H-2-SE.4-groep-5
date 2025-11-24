using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class Capitalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "productBeschrijving",
                table: "Products",
                newName: "ProductBeschrijving");

            migrationBuilder.AlterColumn<string>(
                name: "ProductBeschrijving",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "datum",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "locatie",
                table: "Products",
                type: "nvarchar(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "datum",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "locatie",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "ProductBeschrijving",
                table: "Products",
                newName: "productBeschrijving");

            migrationBuilder.AlterColumn<string>(
                name: "productBeschrijving",
                table: "Products",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
