using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

public class StartAuctionsTests
{
    [Fact]
    public async Task StartAuctions_ReturnsNotFound_WhenNoAuctionsScheduled ()
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
            Datum = DateTime.Today.AddDays(1),
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 2
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();


        var actionResult = await controller.StartAuctions();

        actionResult.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = actionResult as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("No auctions scheduled for today");
    }

    [Fact]
    public async Task StartAuctions_MethodCompletesAndReturnsActivatedProducts_WhenEverythingIsCorrect ()
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


        var actionResult = await controller.StartAuctions();

        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        var activatedProducts = okResult.Value as IEnumerable<object>;
        var list = activatedProducts != null ? new List<object>(activatedProducts) : new List<object>();
        list.Should().NotBeNull();
        product.Status.Should().Be(3);
    }
}