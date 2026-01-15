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

// Tests voor de GetKlokPrijs methode in ProductsController.
// GetKlokPrijs zoekt de meest recente klokprijs-informatie op basis van locatie en status.
// De tests verifiëren scenarios zoals: geen producten op de locatie, geen product met de juiste status,
// en het correct teruggeven van de ClockDTO wanneer er een geldig product is.
public class GetKlokPrijsTests
{
    [Fact]
    public async Task GetKlokPrijs_ReturnsNotFound_WhenNoProductsMatchingLocation ()
    {
        // Setup: in-memory DB isolatie
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisgegevens (rollen en gebruiker)
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Voeg een product toe op locatie B zodat locatie A geen producten heeft
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

        // Act: vraag klokprijs op voor locatie A (geen producten)
        var result = await controller.GetKlokPrijs("A");

        // Assert: verwacht NotFound omdat er geen producten voor locatie A zijn
        result.Result.Should().BeOfType<NotFoundResult>();
    }
    [Fact]
    public async Task GetKlokPrijs_ReturnsNotFound_WhenNoProductMatchingStatus ()
    {
        // Setup in-memory DB
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Voeg een product toe met status die niet overeenkomt (bijv. status 1)
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
        context.SaveChanges();

        // Act: vraag klokprijs op voor locatie B, maar er is geen product met juiste status
        var result = await controller.GetKlokPrijs("B");

        // Assert: verwacht NotFound omdat status niet overeenkomt
        result.Result.Should().BeOfType<NotFoundResult>();
    }
    [Fact]
    public async Task GetKlokPrijs_ReturnsCorrectInfoInDTO_WhenMethodCompletes ()
    {
        // Setup in-memory DB en seed
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Voeg een product toe dat voldoet aan locatie A en de vereiste status
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.UtcNow,
            Locatie = "A",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 3
        };
        context.Products.Add(product);
        context.SaveChanges();

        // Act: vraag klokprijs op voor locatie A
        var result = await controller.GetKlokPrijs("A");

        // Assert: controleer of het resultaat een ClockDTO bevat met correcte waarden
        result.Value.Should().BeOfType<ClockDTO>();
        var dto = result.Value;
        dto.minimumPrijs.Should().Be(product.MinimumPrijs);
        dto.locatie.Should().Be(product.Locatie);
        dto.status.Should().Be(product.Status);
    }
}