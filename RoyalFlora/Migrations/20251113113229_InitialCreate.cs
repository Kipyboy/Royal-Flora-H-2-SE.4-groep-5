using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalFlora.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locatie",
                columns: table => new
                {
                    IdLocatie = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    locatieNaam = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locatie", x => x.IdLocatie);
                });

            migrationBuilder.CreateTable(
                name: "Rollen",
                columns: table => new
                {
                    IdRollen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolNaam = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rollen", x => x.IdRollen);
                });

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    IdStatus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Beschrijving = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.IdStatus);
                });

            migrationBuilder.CreateTable(
                name: "Bedrijf",
                columns: table => new
                {
                    KVK = table.Column<int>(type: "int", nullable: false),
                    BedrijfNaam = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Adress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Oprichter = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bedrijf", x => x.KVK);
                });

            migrationBuilder.CreateTable(
                name: "Gebruiker",
                columns: table => new
                {
                    IdGebruiker = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoorNaam = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    AchterNaam = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Email = table.Column<string>(name: "E-mail", type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Wachtwoord = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Postcode = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Adress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Telefoonnummer = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    KVK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gebruiker", x => x.IdGebruiker);
                    table.ForeignKey(
                        name: "FK_Gebruiker_Bedrijf_KVK",
                        column: x => x.KVK,
                        principalTable: "Bedrijf",
                        principalColumn: "KVK");
                    table.ForeignKey(
                        name: "FK_Gebruiker_Rollen_Rol",
                        column: x => x.Rol,
                        principalTable: "Rollen",
                        principalColumn: "IdRollen");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    IdProduct = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductNaam = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    productBeschrijving = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    MinimumPrijs = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Leverancier = table.Column<int>(type: "int", nullable: true),
                    Koper = table.Column<int>(type: "int", nullable: true),
                    verkoopPrijs = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.IdProduct);
                    table.ForeignKey(
                        name: "FK_Products_Bedrijf_Leverancier",
                        column: x => x.Leverancier,
                        principalTable: "Bedrijf",
                        principalColumn: "KVK");
                    table.ForeignKey(
                        name: "FK_Products_Gebruiker_Koper",
                        column: x => x.Koper,
                        principalTable: "Gebruiker",
                        principalColumn: "IdGebruiker");
                    table.ForeignKey(
                        name: "FK_Products_Status_Status",
                        column: x => x.Status,
                        principalTable: "Status",
                        principalColumn: "IdStatus");
                });

            migrationBuilder.CreateTable(
                name: "Fotos",
                columns: table => new
                {
                    IdFoto = table.Column<int>(type: "int", nullable: false),
                    IdProduct = table.Column<int>(type: "int", nullable: false),
                    FotoPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fotos", x => new { x.IdFoto, x.IdProduct });
                    table.ForeignKey(
                        name: "FK_Fotos_Products_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Products",
                        principalColumn: "IdProduct");
                });

            migrationBuilder.CreateTable(
                name: "Veiling",
                columns: table => new
                {
                    Locatie_idLocatie = table.Column<int>(type: "int", nullable: false),
                    Products_IdProduct = table.Column<int>(type: "int", nullable: false),
                    datum = table.Column<DateTime>(type: "date", nullable: true),
                    ordernummer = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiling", x => new { x.Locatie_idLocatie, x.Products_IdProduct });
                    table.ForeignKey(
                        name: "FK_Veiling_Locatie_Locatie_idLocatie",
                        column: x => x.Locatie_idLocatie,
                        principalTable: "Locatie",
                        principalColumn: "IdLocatie");
                    table.ForeignKey(
                        name: "FK_Veiling_Products_Products_IdProduct",
                        column: x => x.Products_IdProduct,
                        principalTable: "Products",
                        principalColumn: "IdProduct");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bedrijf_Oprichter",
                table: "Bedrijf",
                column: "Oprichter");

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_IdProduct",
                table: "Fotos",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_Gebruiker_KVK",
                table: "Gebruiker",
                column: "KVK");

            migrationBuilder.CreateIndex(
                name: "IX_Gebruiker_Rol",
                table: "Gebruiker",
                column: "Rol");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Koper",
                table: "Products",
                column: "Koper");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Leverancier",
                table: "Products",
                column: "Leverancier");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Veiling_Products_IdProduct",
                table: "Veiling",
                column: "Products_IdProduct");

            migrationBuilder.AddForeignKey(
                name: "FK_Bedrijf_Gebruiker_Oprichter",
                table: "Bedrijf",
                column: "Oprichter",
                principalTable: "Gebruiker",
                principalColumn: "IdGebruiker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bedrijf_Gebruiker_Oprichter",
                table: "Bedrijf");

            migrationBuilder.DropTable(
                name: "Fotos");

            migrationBuilder.DropTable(
                name: "Veiling");

            migrationBuilder.DropTable(
                name: "Locatie");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropTable(
                name: "Gebruiker");

            migrationBuilder.DropTable(
                name: "Bedrijf");

            migrationBuilder.DropTable(
                name: "Rollen");
        }
    }
}
