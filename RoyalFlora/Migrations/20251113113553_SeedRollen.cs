using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class SeedRollen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Rollen",
                columns: new[] { "IdRollen", "RolNaam" },
                values: new object[,]
                {
                    { 1, "Aanvoerder" },
                    { 2, "Inkooper" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rollen",
                keyColumn: "IdRollen",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rollen",
                keyColumn: "IdRollen",
                keyValue: 2);
        }
    }
}
