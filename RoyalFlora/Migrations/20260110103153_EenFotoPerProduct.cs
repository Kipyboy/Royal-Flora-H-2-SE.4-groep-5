using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class EenFotoPerProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Fotos_IdProduct",
                table: "Fotos");

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_IdProduct",
                table: "Fotos",
                column: "IdProduct",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Fotos_IdProduct",
                table: "Fotos");

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_IdProduct",
                table: "Fotos",
                column: "IdProduct");
        }
    }
}
