using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class AantalToegevoegd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "locatie",
                table: "Products",
                newName: "Locatie");

            migrationBuilder.RenameColumn(
                name: "datum",
                table: "Products",
                newName: "Datum");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Locatie",
                table: "Products",
                newName: "locatie");

            migrationBuilder.RenameColumn(
                name: "Datum",
                table: "Products",
                newName: "datum");
        }
    }
}
