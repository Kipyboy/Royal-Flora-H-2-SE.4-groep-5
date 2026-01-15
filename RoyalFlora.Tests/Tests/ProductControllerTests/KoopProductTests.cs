using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

// Tests voor de KoopProduct-methode van de ProductsController.
// De tests verifiëren verschillende scenario's rondom het kopen van een product,
// zoals: product niet gevonden, niet-geauthenticeerde gebruiker, ongeldige gebruiker en succesvolle aankoop.
public class KoopProductTests
{
    [Fact]
    public async Task KoopProduct_ReturnsNotFound_WhenNoProductMatchingId ()
    {
        // Arrange: maak een unieke in-memory database zodat tests geïsoleerd zijn.
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed noodzakelijke data (rollen en een testgebruiker).
        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: maak de controller met de testcontext.
        var controller = new ProductsController(context);

        // Arrange: creëer een geauthenticeerde gebruiker met NameIdentifier = "1".
        // Deze user heeft id 1, maar we vragen een product op met id 2 -> NotFound verwacht.
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

        // Arrange: voeg een product toe met IdProduct = 1
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

        // Arrange: maak de koop request DTO
        var dto = new KoopDto{
            verkoopPrijs = 6.00m
        };

        // Act: probeer een product te kopen dat niet bestaat (id = 2)
        var result = await controller.KoopProduct(2, dto);

        // Assert: verwacht NotFound omdat product id 2 niet bestaat
        result.Should().BeOfType<NotFoundResult>();

    }
    [Fact]
    public async Task KoopProduct_ReturnsUnauthorized_WhenUserIdNull () {
        // Arrange: unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Arrange: geen claims meegegeven om een niet-geauthenticeerde gebruiker te simuleren
        var identity = new ClaimsIdentity(); // geen claims aanwezig
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Arrange: voeg een product toe dat we proberen te kopen
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

        // Act: probeer te kopen zonder geldige gebruiker
        var result = await controller.KoopProduct(1, dto);

        // Assert: verwacht Unauthorized omdat er geen gebruikers-id beschikbaar is
        result.Should().BeOfType<UnauthorizedResult>();

    }
    [Fact]
    public async Task KoopProduct_ReturnsUnauthorized_WhenUserNotFound ()
    {
        // Arrange: unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Arrange: user claim met id = 2, maar er bestaat geen gebruiker met id 2 in de database
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

        // Arrange: voeg een product toe dat we willen kopen
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

        // Act: probeer te kopen met een gebruiker die niet in de DB bestaat
        var result = await controller.KoopProduct(1, dto);

        // Assert: verwacht Unauthorized omdat de gebruiker niet gevonden is
        result.Should().BeOfType<UnauthorizedResult>();

    }
    [Fact]
    public async Task KoopProduct_ReturnsUnauthorized_WhenUserIdParseFails ()
    {
        // Arrange: unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Arrange: user claim met niet-numerieke waarde (parse fout) om foutpad te testen
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

        // Arrange: voeg een product toe
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

        // Act: probeer te kopen met een ongeldige user id (parse fails)
        var result = await controller.KoopProduct(1, dto);

        // Assert: verwachting is Unauthorized bij parse fout
        result.Should().BeOfType<UnauthorizedResult>();

    }
    [Fact]
    public async Task KoopProduct_ReturnsNoContent_WhenMethodCompletes ()
    {
        // Arrange: unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var controller = new ProductsController(context);

        // Arrange: geauthenticeerde gebruiker met id = 1
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

        // Arrange: voeg het product toe dat gekocht zal worden
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

        // Act: voer de aankoop uit
        var result = await controller.KoopProduct(1, dto);

        // Assert: bij succes verwachten we NoContent en dat de status van het product is aangepast (bijv. naar 4)
        result.Should().BeOfType<NoContentResult>();
        var resultProduct = context.Products.First(p => p.IdProduct == 1);
        resultProduct.Status.Should().Be(4);

    }
}