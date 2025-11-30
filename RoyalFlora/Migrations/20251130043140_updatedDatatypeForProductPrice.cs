using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class updatedDatatypeForProductPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumPrijs",
                table: "Products",
                type: "decimal(18,2)",
                maxLength: 45,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MinimumPrijs",
                table: "Products",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldMaxLength: 45);
        }
    }
}
