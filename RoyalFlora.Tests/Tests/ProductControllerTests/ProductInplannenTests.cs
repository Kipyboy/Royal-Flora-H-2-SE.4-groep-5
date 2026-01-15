using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

// Tests voor de productInplannen-methode van de ProductsController.
// De tests controleren verschillende foutgevallen (niet gevonden, ongeldige datum, ongeldige startprijs)
// en het succesvolle pad waarbij een product gepland wordt (Status verandert naar 2).
public class ProductInplannenTests
{
    [Fact]
    public async Task ProductInplannen_ReturnsNotFound_WhenNoProductFound ()
    {
        // Arrange: unieke in-memory database per test
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed benodigde data (rollen en testgebruiker)
        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: maak de controller met de testcontext
        var controller = new ProductsController(context);

        // Arrange: voeg een product toe met IdProduct = 1
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 3
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act: probeer product in te plannen met een niet-bestaand product id (2)
        var actionResult = await controller.productInplannen(2, null, null);

        // Assert: verwacht NotFound met de correcte foutmelding
        actionResult.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = actionResult as NotFoundObjectResult;
        var message = notFoundResult.Value;
        message.Should().Be("No valid product found");
    }
    [Fact]
    public async Task ProductInplannen_ReturnsBadRequestWithCorrectMessage_WhenIncorrectDate ()
    {
        // Arrange: unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed data
        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Arrange: voeg product toe dat bestaat maar we geven een ongeldige datum door
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 1
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Arrange: geldige startprijs maar ongeldige datum string
        string startprijs = "1.00";

        // Act: roep productInplannen aan met een ongeldige datum
        var actionResult = await controller.productInplannen(1, "dfeosiafjoi", startprijs);

        // Assert: verwacht BadRequest met message "Ongeldige datum/tijd"
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var BadRequestResult = actionResult as BadRequestObjectResult;
        var message = BadRequestResult.Value;
        message.Should().Be("Ongeldige datum/tijd");
    }

    [Fact]
    public async Task ProductInplannen_ReturnsBadRequestWithCorrectMessage_WhenIncorrectStartPrice ()
    {
        // Arrange: unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Arrange: voeg product toe
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 1
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Arrange: geldige datum string maar startprijs is onjuist (niet-parsebaar)
        var today = DateTime.Today;
        string datum = today.ToString("yyyy-MM-dd HH:mm");
        string startprijs = "odijaiofifo";

        // Act: probeer in te plannen met een ongeldige startprijs
        var actionResult = await controller.productInplannen(1, datum, startprijs);

        // Assert: verwacht BadRequest met message "Ongeldige startprijs"
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var BadRequestResult = actionResult as BadRequestObjectResult;
        var message = BadRequestResult.Value;
        message.Should().Be("Ongeldige startprijs");
    }

    [Fact]
    public async Task ProductInplannen_MethodCompletes_WhenEverythingIsCorrect ()
    {
        // Arrange: unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Arrange: voeg product toe dat gepland moet worden
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 1
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Arrange: geldige datum en startprijs
        var today = DateTime.Today;
        string datum = today.ToString("yyyy-MM-dd HH:mm");
        string startprijs = "1.00";

        // Act: plan het product in
        var actionResult = await controller.productInplannen(1, datum, startprijs);

        // Assert: bij succes verwachten we Ok en dat de status verandert naar 2 (gepland)
        actionResult.Should().BeOfType<OkObjectResult>();
        product.Status.Should().Be(2);
    }
}