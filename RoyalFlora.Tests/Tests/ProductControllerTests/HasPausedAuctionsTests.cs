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

public class HasPausedAuctionsTests
{
    [Fact]
    public async Task HasPausedAuctions_ReturnsTrue_WhenPausedProductExists ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

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

        var actionResult = await controller.HasPausedAuctions();

        var okResult = actionResult.Result as OkObjectResult;
        bool anyPaused = (bool)okResult.Value;
        anyPaused.Should().BeTrue();
    }
    [Fact]
    public async Task HasPausedAuctions_ReturnsFalse_WhenPausedProductDoesNotExist ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

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

        var actionResult = await controller.HasPausedAuctions();

        var okResult = actionResult.Result as OkObjectResult;
        bool anyPaused = (bool)okResult.Value;
        anyPaused.Should().BeFalse();
    }
}