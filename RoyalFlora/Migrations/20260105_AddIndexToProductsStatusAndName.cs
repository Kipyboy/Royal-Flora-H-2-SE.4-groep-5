using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToProductsStatusAndName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create index on Status and ProductNaam columns
            // This index will speed up queries filtering by Status = 4 with ProductNaam
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes 
                WHERE name = 'IX_Products_Status_ProductNaam' AND object_id = OBJECT_ID('Products'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_Products_Status_ProductNaam 
                    ON Products (Status, ProductNaam)
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes 
                WHERE name = 'IX_Products_Status_ProductNaam' AND object_id = OBJECT_ID('Products'))
                BEGIN
                    DROP INDEX IX_Products_Status_ProductNaam ON Products
                END
            ");
        }
    }
}
