using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class VeilingmeestersUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF COL_LENGTH('Products','StartPrijs') IS NULL ALTER TABLE Products ADD [StartPrijs] nvarchar(max) NULL;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Rollen WHERE IdRollen = 3)
BEGIN
    SET IDENTITY_INSERT Rollen ON;
    INSERT INTO Rollen (IdRollen, RolNaam) VALUES (3, 'Veilingmeester');
    SET IDENTITY_INSERT Rollen OFF;
END;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Status WHERE IdStatus = 1)
BEGIN
    SET IDENTITY_INSERT Status ON;
    INSERT INTO Status (IdStatus, Beschrijving) VALUES (1, 'Geregistreerd');
    SET IDENTITY_INSERT Status OFF;
END;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Status WHERE IdStatus = 2)
BEGIN
    SET IDENTITY_INSERT Status ON;
    INSERT INTO Status (IdStatus, Beschrijving) VALUES (2, 'Ingepland');
    SET IDENTITY_INSERT Status OFF;
END;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Status WHERE IdStatus = 3)
BEGIN
    SET IDENTITY_INSERT Status ON;
    INSERT INTO Status (IdStatus, Beschrijving) VALUES (3, 'Geveild');
    SET IDENTITY_INSERT Status OFF;
END;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM Status WHERE IdStatus = 4)
BEGIN
    SET IDENTITY_INSERT Status ON;
    INSERT INTO Status (IdStatus, Beschrijving) VALUES (4, 'Verkocht');
    SET IDENTITY_INSERT Status OFF;
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rollen",
                keyColumn: "IdRollen",
                keyValue: 3);

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

            migrationBuilder.Sql(
                "IF COL_LENGTH('Products','StartPrijs') IS NOT NULL ALTER TABLE Products DROP COLUMN StartPrijs;");
        }
    }
}
