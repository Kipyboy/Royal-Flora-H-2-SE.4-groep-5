using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

public class KoopProductTests
{
    [Fact]
    public async Task KoopProduct_ReturnsNotFound_WhenNoProductMatchingId ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Create a ClaimsPrincipal with a non-numeric NameIdentifier claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

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

        var dto = new KoopDto{
            verkoopPrijs = 6.00m
        };

        var result = await controller.KoopProduct(2, dto);

        result.Should().BeOfType<NotFoundResult>();

    }
    [Fact]
    public async Task KoopProduct_ReturnsUnauthorized_WhenUserIdNull () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        
        var identity = new ClaimsIdentity(); // no claims provided
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

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

        var dto = new KoopDto{
            verkoopPrijs = 6.00m
        };

        var result = await controller.KoopProduct(1, dto);

        result.Should().BeOfType<UnauthorizedResult>();

    }
    [Fact]
    public async Task KoopProduct_ReturnsUnauthorized_WhenUserNotFound ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Create a ClaimsPrincipal with a non-numeric NameIdentifier claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "2")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

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

        var dto = new KoopDto{
            verkoopPrijs = 6.00m
        };

        var result = await controller.KoopProduct(1, dto);

        result.Should().BeOfType<UnauthorizedResult>();

    }
    [Fact]
    public async Task KoopProduct_ReturnsUnauthorized_WhenUserIdParseFails ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Create a ClaimsPrincipal with a non-numeric NameIdentifier claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "A")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

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

        var dto = new KoopDto{
            verkoopPrijs = 6.00m
        };

        var result = await controller.KoopProduct(1, dto);

        result.Should().BeOfType<UnauthorizedResult>();

    }
    [Fact]
    public async Task KoopProduct_ReturnsNoContent_WhenMethodCompletes ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Create a ClaimsPrincipal with a non-numeric NameIdentifier claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

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

        var dto = new KoopDto{
            verkoopPrijs = 6.00m
        };

        var result = await controller.KoopProduct(1, dto);

        result.Should().BeOfType<NoContentResult>();
        var resultProduct = context.Products.First(p => p.IdProduct == 1);
        resultProduct.Status.Should().Be(4);

    }
}