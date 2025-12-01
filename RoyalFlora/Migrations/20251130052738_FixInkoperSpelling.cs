using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class FixInkoperSpelling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rollen",
                keyColumn: "IdRollen",
                keyValue: 2,
                column: "RolNaam",
                value: "Inkoper");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rollen",
                keyColumn: "IdRollen",
                keyValue: 2,
                column: "RolNaam",
                value: "Inkooper");
        }
    }
}
