using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoyalFlora.Tests.Helpers;
using Xunit;
using RoyalFlora.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace RoyalFlora.Tests.Tests.ProductControllerTests
{
    // Tests voor het registreren (posten) van producten via de ProductsController.
    // Deze klasse bevat tests voor het succesvolle insert-pad en verschillende foutpaden.
    public class ProductRegisterTest
    {
        [Fact]
        public async Task PostProduct_ReturnCreatedAtAction_WhenObjectIsInserted()
        {
            // Arrange: maak een unieke in-memory database zodat testen geïsoleerd zijn.
            var dbName = Guid.NewGuid().ToString();
            using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Arrange: seed rollen en maak een testgebruiker aan (noodzakelijk voor bedrijf en eigenaar).
            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Arrange: maak configuration en controllers (AuthController niet direct gebruikt hier, maar seeded).
            var configuration = TestHelpers.CreateTestConfiguration();
            var AuthController = new AuthController(context, configuration);

            var ProductController = new ProductsController(context);

            // Arrange: seed een bedrijf gekoppeld aan de testgebruiker zodat producten aan een bedrijf kunnen worden toegevoegd.
            TestHelpers.SeedBedrijf(context, gebruiker);
            

            // Act: voer de PostProduct aanroep uit met geldige parameters
            var actionResult = await ProductController.PostProduct(
                ProductNaam: "TestProduct",
                ProductBeschrijving: "Test product beschrijving",
                MinimumPrijs: "5",
                Locatie: "Naaldwijk",
                Datum: "2025-12-20",
                Aantal: "1",
                Leverancier: "87654321",
                images: new List<IFormFile>()
            );

            // Assert: controleer dat er een resultaat is en dat het een CreatedAtActionResult is (succesvolle creatie)
            actionResult.Result.Should().NotBeNull("PostProduct should return a result");
            actionResult.Result.Should().BeOfType<CreatedAtActionResult>();
            
            // Assert: controleer dat het product daadwerkelijk in de database is opgeslagen en dat velden correct zijn gezet
            var createdProduct = await context.Products
                .FirstOrDefaultAsync(p => p.ProductNaam == "TestProduct");
            
            createdProduct.Should().NotBeNull("Product should be created in database");
            createdProduct!.ProductNaam.Should().Be("TestProduct");
            createdProduct.ProductBeschrijving.Should().Be("Test product beschrijving");
            createdProduct.MinimumPrijs.Should().Be(5);
            createdProduct.Locatie.Should().Be("Naaldwijk");
            createdProduct.Aantal.Should().Be(1);
            createdProduct.Leverancier.Should().Be(87654321);
        }

        [Fact]
        public async Task PostProduct_ReturnsBadRequest_WhenMinimumPrijsInvalid()
        {
            // Arrange: unieke in-memory database
            var dbName = Guid.NewGuid().ToString();
            using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Arrange: seed rollen en maak testgebruiker
            TestHelpers.SeedRollen(context);
            var gebruiker = TestHelpers.SeedUser(context, "test2@gmail.com", "test123!");

            var productController = new ProductsController(context);

            // Act: probeer een product te posten met een ongeldige MinimumPrijs (niet parsebaar naar nummer)
            var actionResult = await productController.PostProduct(
                ProductNaam: "BadPriceProduct",
                ProductBeschrijving: "Bad price",
                MinimumPrijs: "not-a-number",
                Locatie: "Loc",
                Datum: "2025-12-20",
                Aantal: "1",
                Leverancier: "",
                images: new List<IFormFile>()
            );

            // Assert: verwacht BadRequest vanwege ongeldige prijs
            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task PostProduct_ReturnsBadRequest_WhenDatumInvalid()
        {
            // Arrange: unieke in-memory database
            var dbName = Guid.NewGuid().ToString();
            using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Arrange: seed rollen en maak testgebruiker
            TestHelpers.SeedRollen(context);
            var gebruiker = TestHelpers.SeedUser(context, "test3@gmail.com", "test123!");

            var productController = new ProductsController(context);

            // Act: probeer een product te posten met een ongeldige datum-string
            var actionResult = await productController.PostProduct(
                ProductNaam: "BadDateProduct",
                ProductBeschrijving: "Bad date",
                MinimumPrijs: "5",
                Locatie: "Loc",
                Datum: "not-a-date",
                Aantal: "1",
                Leverancier: "",
                images: new List<IFormFile>()
            );

            // Assert: verwacht BadRequest omdat de datum niet parsebaar is
            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
            
 }
