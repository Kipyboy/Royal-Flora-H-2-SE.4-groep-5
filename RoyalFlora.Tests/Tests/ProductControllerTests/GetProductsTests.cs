using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

// Tests voor de GetProducts endpoint in ProductsController.
// Deze tests controleren:
// - validatie van de gebruiker via claims (parsing van NameIdentifier)
// - gedrag als gebruiker niet gevonden is
// - en dat verschillende producttypes correct gemapped worden naar ProductDTO (gekocht, eigen, normaal)
public class GetProductsTests
{
    [Fact]
    public async Task GetProducts_ReturnsUnauthorized_WhenUserIdCanNotBeParsed()
    {
        // in-memory DB per test voor isolatie
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // seed basisgegevens (rollen en gebruikers)
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Maak een ClaimsPrincipal met een niet-numerieke NameIdentifier zodat parsing faalt
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

        // Act: vraag producten op
        var result = await controller.GetProducts(null);

        // Assert: verwacht Unauthorized omdat userId niet te parsen is
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
    [Fact]
    public async Task GetProducts_ReturnsUnauthorized_WhenUserIdClaimMissing () {
        // in-memory DB
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Maak een ClaimsPrincipal zonder NameIdentifier claim
        var claims = new List<Claim>
        {
            
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await controller.GetProducts(null);

        // Assert: verwacht Unauthorized omdat de NameIdentifier claim ontbreekt
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
    [Fact]
    public async Task GetProducts_ReturnsUnauthorized_WhenNoUserFound () {
        // in-memory DB
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Maak een ClaimsPrincipal met een userId die niet bestaat (2)
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

        // Act
        var result = await controller.GetProducts(null);

        // Assert: verwacht Unauthorized omdat de gebruiker niet in de DB is gevonden
        result.Result.Should().BeOfType<UnauthorizedResult>();
    }
    [Fact]
    public async Task GetProducts_ReturnsGekochtProduct_WhenStatusIsGekocht ()
    {
        // in-memory DB en seed
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
        TestHelpers.SeedBedrijf(context, gebruiker);

        var controller = new ProductsController(context);

        // ClaimsPrincipal voor gebruiker met Id 1
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        // Product met status 'gekocht' en een foto
        var product = new Product
        {
            ProductNaam = "GekochtProduct",
            ProductBeschrijving = "Beschrijving van gekocht product",
            verkoopPrijs = 150.50m,
            MinimumPrijs = 100m,
            Locatie = "A",
            Datum = DateTime.UtcNow,
            Aantal = 2,
            Foto = new Foto {FotoPath = "img.jpg" } ,
            StatusNavigation = new Status { Beschrijving = "gekocht" }
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // persist the product in the in-memory context so the controller can load it
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetProducts("A");

        // Assert: controleer DTO mapping en waarden
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
        // in-memory DB en seed
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
        TestHelpers.SeedBedrijf(context, gebruiker);

        var controller = new ProductsController(context);

        // ClaimsPrincipal voor gebruiker met Id 1
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
            Foto = new Foto {FotoPath = "img.jpg" } ,
            StatusNavigation = new Status { Beschrijving = "gekocht" }
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // persist the product in the in-memory context so the controller can load it
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetProducts("A");

        // Assert: controleer DTO mapping en dat type 'eigen' is
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
        // in-memory DB en seed
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
        TestHelpers.SeedBedrijf(context, gebruiker);

        var controller = new ProductsController(context);

        // ClaimsPrincipal voor gebruiker met Id 1
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
            Foto = new Foto {FotoPath = "img.jpg" } ,
            StatusNavigation = new Status { Beschrijving = "aankomend" }
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // persist the product in the in-memory context so the controller can load it
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetProducts("A");

        // Assert: controleer DTO mapping en default type (leeg)
        var returned = result.Value;
        returned.Should().NotBeNull();

        var list = new List<ProductDTO>(returned);
        list.Should().NotBeEmpty();

        var dto = list[0];
        dto.fotoPath.Should().Be("img.jpg");
        dto.type.Should().BeEmpty();
    }
 }
