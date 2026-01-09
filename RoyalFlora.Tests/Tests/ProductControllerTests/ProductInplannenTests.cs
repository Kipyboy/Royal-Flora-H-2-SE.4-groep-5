using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

public class ProductInplannenTests
{
    [Fact]
    public async Task ProductInplannen_ReturnsNotFound_WhenNoProductFound ()
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
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 3
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var actionResult = await controller.productInplannen(2, null, null);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = actionResult as NotFoundObjectResult;
        var message = notFoundResult.Value;
        message.Should().Be("No valid product found");
    }
    [Fact]
    public async Task ProductInplannen_ReturnsBadRequestWithCorrectMessage_WhenIncorrectDate ()
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
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 1
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        string startprijs = "1.00";

        var actionResult = await controller.productInplannen(1, "dfeosiafjoi", startprijs);

        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var BadRequestResult = actionResult as BadRequestObjectResult;
        var message = BadRequestResult.Value;
        message.Should().Be("Ongeldige datum/tijd");
    }

    [Fact]
    public async Task ProductInplannen_ReturnsBadRequestWithCorrectMessage_WhenIncorrectStartPrice ()
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
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 1
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var today = DateTime.Today;
        string datum = today.ToString("yyyy-MM-dd HH:mm");
        string startprijs = "odijaiofifo";

        var actionResult = await controller.productInplannen(1, datum, startprijs);

        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var BadRequestResult = actionResult as BadRequestObjectResult;
        var message = BadRequestResult.Value;
        message.Should().Be("Ongeldige startprijs");
    }

    [Fact]
    public async Task ProductInplannen_MethodCompletes_WhenEverythingIsCorrect ()
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
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 1
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var today = DateTime.Today;
        string datum = today.ToString("yyyy-MM-dd HH:mm");
        string startprijs = "1.00";

        var actionResult = await controller.productInplannen(1, datum, startprijs);

        actionResult.Should().BeOfType<OkObjectResult>();
        product.Status.Should().Be(2);
    }
}