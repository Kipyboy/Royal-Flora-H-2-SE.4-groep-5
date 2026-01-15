using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

// Tests voor de HasPausedAuctions-methode van de ProductsController.
// Deze tests controleren of er (a) minstens één gepauzeerde veiling bestaat (Status == 5)
// en (b) dat er geen gepauzeerde veilingen bestaan wanneer alleen andere statussen aanwezig zijn.
public class HasPausedAuctionsTests
{
    [Fact]
    public async Task HasPausedAuctions_ReturnsTrue_WhenPausedProductExists ()
    {
        // Arrange: maak een unieke in-memory database zodat tests geïsoleerd zijn.
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed benodigde data zoals rollen en een testgebruiker.
        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: maak de controller met de testcontext.
        var controller = new ProductsController(context);

        // Arrange: voeg een product toe met Status = 5 (aangeduid als 'gepauzeerd').
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.Today,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 5
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act: roep de methode aan die controleert of gepauzeerde veilingen bestaan.
        var actionResult = await controller.HasPausedAuctions();

        // Assert: haal het OkObjectResult uit het action result en controleer de boolean waarde.
        var okResult = actionResult.Result as OkObjectResult;
        bool anyPaused = (bool)okResult.Value;
        anyPaused.Should().BeTrue();
    }
    [Fact]
    public async Task HasPausedAuctions_ReturnsFalse_WhenPausedProductDoesNotExist ()
    {
        // Arrange: maak een unieke in-memory database zodat tests onafhankelijk zijn.
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed rollen en een testgebruiker.
        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: maak de controller met de testcontext.
        var controller = new ProductsController(context);

        // Arrange: voeg een product toe met Status != 5 (geen gepauzeerde veiling).
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.Today,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 2
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act: roep de methode aan om te controleren of gepauzeerde veilingen aanwezig zijn.
        var actionResult = await controller.HasPausedAuctions();

        // Assert: controleer dat de geretourneerde boolean false is wanneer geen Status == 5 producten bestaan.
        var okResult = actionResult.Result as OkObjectResult;
        bool anyPaused = (bool)okResult.Value;
        anyPaused.Should().BeFalse();
    }
}