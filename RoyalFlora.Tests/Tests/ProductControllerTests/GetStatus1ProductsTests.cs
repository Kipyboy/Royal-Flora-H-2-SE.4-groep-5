using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

public class GetStatus1ProductsTests
{
    [Fact]
    public async Task GetStatus1Products_CompletesWithoutErrors_WhenNoStatus1Products ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        var actionResult = await controller.GetStatus1Products();

        var list = actionResult.Value;

        list.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStatus1Products_ReturnedListContainsProducts_AfterMethodCompletion ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        var testProduct1 = new Product
        {
            ProductNaam = "Test Product 1",
            MinimumPrijs = 10m,
            Status = 1,
            Locatie = "LocatieA"
        };

        var testProduct2 = new Product
        {
            ProductNaam = "Test Product 2",
            MinimumPrijs = 20m,
            Status = 1,
            Locatie = "LocatieB"
        };

        context.Products.AddRange(testProduct1, testProduct2);
        context.SaveChanges();

        var actionResult = await controller.GetStatus1Products();

        var list = actionResult.Value;

        list.Should().NotBeNull();
        list.Should().HaveCount(2);
        list.Should().Contain(p => p.naam == "Test Product 1");
        list.Should().Contain(p => p.naam == "Test Product 2");
    }
}