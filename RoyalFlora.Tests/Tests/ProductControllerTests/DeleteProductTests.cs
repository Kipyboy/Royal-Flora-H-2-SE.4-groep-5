using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

namespace RoyalFlora.Tests.Tests.ProductControllerTests;

// Tests voor de DeleteProduct methode in de ProductsController.
// Deze tests controleren de situatie waarbij:
// - een gevraagde product-id niet bestaat -> 404 NotFound
// - het verwijderen succesvol is -> 204 NoContent en het product is verwijderd uit de DB
public class DeleteProductTests
{
    [Fact]
    public async Task DeleteProduct_ReturnsNotFound_WhenProductIsNull ()
    {
        // Unieke in-memory database voor isolatie
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisgegevens (rollen en een gebruiker), zodat controller dependencies aanwezig zijn
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Voeg één product toe met Id 1
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.UtcNow,
            Locatie = "TestLocatie",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 1
        };
        context.Products.Add(product);
        context.SaveChanges();

        // Act: probeer product met Id 2 te verwijderen (bestaat niet)
        var result = await controller.DeleteProduct(2);

        // Assert: verwacht NotFound omdat product niet bestaat
        result.Should().BeOfType<NotFoundResult>();
    }
    [Fact]
    public async Task DeleteProduct_ReturnsNoContent_WhenMethodCompletes ()
    {
        // In-memory DB en benodigde seed
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Voeg product toe dat we later gaan verwijderen
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.UtcNow,
            Locatie = "TestLocatie",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 1
        };
        context.Products.Add(product);
        context.SaveChanges();

        // Act: verwijder het aangemaakte product
        var result = await controller.DeleteProduct(1);

        // Assert: verwacht NoContent en dat de lijst met producten leeg is
        result.Should().BeOfType<NoContentResult>();

        context.Products.Should().BeEmpty();
    }
}