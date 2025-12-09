using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

public class GetProductsTests
{
    [Fact]
    public async Task GetProducts_ReturnsUnauthorized_WhenUserIdCanNotBeParsed()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Create a ClaimsPrincipal with a non-numeric NameIdentifier claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "not-an-int")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var result = await controller.GetProducts(null);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
    [Fact]
    public async Task GetProducts_ReturnsUnauthorized_WhenUserIdClaimMissing () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Create a ClaimsPrincipal with a non-numeric NameIdentifier claim
        var claims = new List<Claim>
        {
            
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var result = await controller.GetProducts(null);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
    [Fact]
    public async Task GetProducts_ReturnsUnauthorized_WhenNoUserFound () {
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

        var result = await controller.GetProducts(null);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
    [Fact]
    public async Task GetProducts_ReturnsGekochtProduct_WhenStatusIsGekocht ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
        TestHelpers.SeedBedrijf(context, gebruiker);

        var controller = new ProductsController(context);

        // Create a ClaimsPrincipal with a non-numeric NameIdentifier claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var product = new Product
        {
            ProductNaam = "GekochtProduct",
            ProductBeschrijving = "Beschrijving van gekocht product",
            verkoopPrijs = 150.50m,
            MinimumPrijs = 100m,
            Locatie = "A",
            Datum = DateTime.UtcNow,
            Aantal = 2,
            Fotos = new List<Foto> { new Foto { FotoPath = "img.jpg" } },
            StatusNavigation = new Status { Beschrijving = "gekocht" }
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // persist the product in the in-memory context so the controller can load it
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var result = await controller.GetProducts("A");

        // The controller returns an ActionResult<IEnumerable<ProductDTO>>; the successful value is in `result.Value`
        var returned = result.Value;
        returned.Should().NotBeNull();

        var list = new List<ProductDTO>(returned);
        list.Should().NotBeEmpty();

        var dto = list[0];
        dto.status.Should().Be("gekocht");
        dto.verkoopPrijs.Should().Be(150.50m);
        dto.fotoPath.Should().Be("img.jpg");
        dto.type.Should().Be("gekocht");
    }
    [Fact]
    public async Task GetProducts_ReturnsEigenProduct_WhenLeverancierAndUserAreTheSame () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
        TestHelpers.SeedBedrijf(context, gebruiker);

        var controller = new ProductsController(context);

        // Create a ClaimsPrincipal with a non-numeric NameIdentifier claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var product = new Product
        {
            ProductNaam = "EigenProduct",
            ProductBeschrijving = "Beschrijving van eigen product",
            Leverancier = 87654321,
            verkoopPrijs = 150.50m,
            MinimumPrijs = 100m,
            Locatie = "A",
            Datum = DateTime.UtcNow,
            Aantal = 2,
            Fotos = new List<Foto> { new Foto { FotoPath = "img.jpg" } },
            StatusNavigation = new Status { Beschrijving = "gekocht" }
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // persist the product in the in-memory context so the controller can load it
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var result = await controller.GetProducts("A");

        // The controller returns an ActionResult<IEnumerable<ProductDTO>>; the successful value is in `result.Value`
        var returned = result.Value;
        returned.Should().NotBeNull();

        var list = new List<ProductDTO>(returned);
        list.Should().NotBeEmpty();

        var dto = list[0];
        dto.status.Should().Be("gekocht");
        dto.verkoopPrijs.Should().Be(150.50m);
        dto.fotoPath.Should().Be("img.jpg");
        dto.type.Should().Be("eigen");
    }
    [Fact]
    public async Task GetProducts_ReturnsNormalProduct_WhenNotEigenOrGekocht () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
        TestHelpers.SeedBedrijf(context, gebruiker);

        var controller = new ProductsController(context);

        // Create a ClaimsPrincipal with a non-numeric NameIdentifier claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var product = new Product
        {
            ProductNaam = "Product",
            ProductBeschrijving = "Beschrijving van product",
            MinimumPrijs = 100m,
            Locatie = "A",
            Datum = DateTime.UtcNow,
            Aantal = 2,
            Fotos = new List<Foto> { new Foto { FotoPath = "img.jpg" } },
            StatusNavigation = new Status { Beschrijving = "aankomend" }
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // persist the product in the in-memory context so the controller can load it
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var result = await controller.GetProducts("A");

        // The controller returns an ActionResult<IEnumerable<ProductDTO>>; the successful value is in `result.Value`
        var returned = result.Value;
        returned.Should().NotBeNull();

        var list = new List<ProductDTO>(returned);
        list.Should().NotBeEmpty();

        var dto = list[0];
        dto.fotoPath.Should().Be("img.jpg");
        dto.type.Should().BeEmpty();
    }
 }
