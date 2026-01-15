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

namespace RoyalFlora.Tests.Tests.ProductControllerTests {

    // Tests voor de Advance-methode van de ProductsController.
    // De Advance-methode probeert een product in een rij (per locatie) door te zetten naar de volgende stap.
    // Deze tests gebruiken een in-memory database en controleren diverse randgevallen zoals:
    // - geen actief product op de gegeven locatie
    // - geen volgend product beschikbaar
    // - succesvolle voortgang van het product
    public class AdvanceTests
    {
    [Fact]
    public async Task Advance_ReturnsNotFound_WhenCurrentProductNotFound()
    {
        // Unieke in-memory database voor isolatie tussen tests
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisgegevens (rollen en gebruiker) zodat de controller afhankelijkheden heeft
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Voeg een product toe op locatie B met status 2 (niet actief op A)
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
            Status = 2
        };
        context.Products.Add(product);
        context.SaveChanges();

        // Act: vraag Advance aan voor locatie A (waar geen actief product is)
        var result = await controller.Advance("A");

        // Assert: verwacht NotFound met specifieke foutmelding
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFound = result as NotFoundObjectResult;
        notFound!.Value.Should().Be("No active product found");
    }
    [Fact]
    public async Task Advance_ReturnsNotFound_WhenNoNextProduct () {
        // Setup similar aan bovenstaand, maar nu is er een actief product zonder opvolger
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Product met status 3 (bijvoorbeeld laatste status), dus er is geen volgend product
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
        context.SaveChanges();

        // Act: vraag Advance aan voor locatie B
        var result = await controller.Advance("B");

        // Assert: verwacht No next product available
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFound = result as NotFoundObjectResult;
        notFound!.Value.Should().Be("No next product available");
    }
    [Fact]
    public async Task Advance_ReturnsOk_WhenMethodCompletes () {
        // Setup waarbij er een actief product is dat opgevolgd kan worden door een ander product
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Eerste product (bijv. huidige actief)
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
        // Tweede product dat de opvolger kan zijn
        var product2 = new Product {
            IdProduct = 2,
            ProductNaam = "Test Bloem2",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 2
        };
        context.Products.Add(product);
        context.Products.Add(product2);
        context.SaveChanges();

        // Act: probeer advance op locatie B
        var result = await controller.Advance("B");

        // Assert: verwacht Ok wanneer methode succesvol is uitgevoerd
        result.Should().BeOfType<OkObjectResult>();


    }
}
}