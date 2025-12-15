using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class SeedStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Status",
                columns: new[] { "IdStatus", "Beschrijving" },
                values: new object[,]
                {
                    { 1, "In te plannen" },
                    { 2, "Aankomend" },
                    { 3, "Actief" },
                    { 4, "Gekocht" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Status",
                keyColumn: "IdStatus",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Status",
                keyColumn: "IdStatus",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Status",
                keyColumn: "IdStatus",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Status",
                keyColumn: "IdStatus",
                keyValue: 4);
        }
    }
}
