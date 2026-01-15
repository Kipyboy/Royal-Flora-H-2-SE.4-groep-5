using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

// Tests voor de StartAuctions-methode van de ProductsController.
// De tests controleren of geplande veilingen correct worden gestart op de juiste dag,
// en dat er een foutmelding wordt geretourneerd wanneer er geen veilingen voor vandaag gepland zijn.
public class StartAuctionsTests
{
    [Fact]
    public async Task StartAuctions_ReturnsNotFound_WhenNoAuctionsScheduled ()
    {
        // Arrange: unieke in-memory database per test
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed rollen en maak een testgebruiker
        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: maak de controller met de testcontext
        var controller = new ProductsController(context);

        // Arrange: voeg een product toe met Datum = morgen zodat het niet vandaag gestart wordt
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.Today.AddDays(1),
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 2
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act: probeer veilingen te starten voor vandaag
        var actionResult = await controller.StartAuctions();

        // Assert: verwacht NotFound met de juiste foutmelding wanneer er geen veilingen voor vandaag zijn
        actionResult.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = actionResult as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("No auctions scheduled for today");
    }

    [Fact]
    public async Task StartAuctions_MethodCompletesAndReturnsActivatedProducts_WhenEverythingIsCorrect ()
    {
        // Arrange: unieke in-memory database per test
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed rollen en maak een testgebruiker
        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: maak de controller met de testcontext
        var controller = new ProductsController(context);

        // Arrange: voeg een product toe met Datum = vandaag en Status = 2 (gepland)
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

        // Act: start veilingen die vandaag gepland zijn
        var actionResult = await controller.StartAuctions();

        // Assert: controleer dat er een Ok-resultaat is en dat de geretourneerde lijst niet null is
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        var activatedProducts = okResult.Value as IEnumerable<object>;
        var list = activatedProducts != null ? new List<object>(activatedProducts) : new List<object>();
        list.Should().NotBeNull();

        // Assert: controleer dat het product geactiveerd is (status veranderd naar 3)
        product.Status.Should().Be(3);
    }
}