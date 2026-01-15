using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RoyalFlora.Tests.Tests.AuthControllerTests;

// Tests voor de UpdateBedrijfInfo methode in de AuthController.
// De tests gebruiken een in-memory database en controleren verschillende randgevallen:
// - Geen ingelogde gebruiker -> 401 Unauthorized
// - Ingelogde gebruiker die niet bestaat -> 404 NotFound
// - Ongeldige veldnaam voor update -> 400 BadRequest
// - Geldige update -> 200 Ok en controle of de wijziging is doorgevoerd
public class UpdateBedrijfInfoTests
{
    [Fact]
    public async Task UpdateBedrijfInfo_ReturnsUnauthorized_WhenUserNotLoggedIn()
    {
        // Unieke in-memory database per test om bijvangsten tussen tests te voorkomen
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed belangrijke referentiegegevens (rollen en gebruikers)
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Mock van configuratie, we hebben geen echte config nodig voor deze tests
        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Simuleer een lege (niet-ingelogde) gebruiker
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        // Voorbeeld request om bedrijfsinformatie aan te passen
        var request = new AuthDTO.UpdateBedrijfInfoRequest
        {
            Field = "bedrijfnaam",
            NewValue = "changeTest"
        };

        // Act: roep de controller methode aan
        var actionResult = await controller.UpdateBedrijfInfo(request);

        // Assert: verwacht Unauthorized (geen ingelogde gebruiker)
        actionResult.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdateBedrijfInfo_ReturnsNotFound_WhenUserNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Simuleer een ingelogde user met een ID die niet in de DB voorkomt
        var userId = 12345;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        var request = new AuthDTO.UpdateBedrijfInfoRequest
        {
            Field = "bedrijfnaam",
            NewValue = "changeTest"
        };

        // Act: roep de methode aan met een niet-bestaande gebruiker
        var actionResult = await controller.UpdateBedrijfInfo(request);

        // Assert: verwacht NotFound omdat de gebruiker niet bestaat
        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateBedrijfInfo_ReturnsBadRequest_WhenInvalidField()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed data
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Gebruik een bestaande gebruiker (Id 1 wordt door TestHelpers aangemaakt)
        var userId = 1;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        // Vraag om update van een ongeldig veld
        var request = new AuthDTO.UpdateBedrijfInfoRequest
        {
            Field = "invalidfield",
            NewValue = "changeTest"
        };

        // Act: roep de methode aan
        var actionResult = await controller.UpdateBedrijfInfo(request);

        // Assert: verwacht BadRequest vanwege ongeldig veld
        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateBedrijfInfo_ReturnsOkAndUpdatesBedrijfCorrectly_WhenMethodCompletes()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed rollen en maak een testgebruiker aan
        TestHelpers.SeedRollen(context);
        var user = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Maak een bedrijf aan en koppel deze aan de gebruiker (KVK-nummer willekeurig gegenereerd)
        var kvk = new Random().Next(10000000, 99999999);
        var bedrijf = new Bedrijf
        {
            KVK = kvk,
            BedrijfNaam = "OrigNaam",
            Adress = "OrigAdres",
            Postcode = "0000AA",
            Oprichter = user.IdGebruiker
        };
        context.Bedrijven.Add(bedrijf);

        // Koppel KVK aan gebruiker en sla op
        user.KVK = kvk;
        context.Gebruikers.Update(user);
        await context.SaveChangesAsync();

        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Simuleer ingelogde gebruiker (deze bestaat nu wel in de DB)
        var userId = user.IdGebruiker;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        // Verzoek om bedrijfsnaam te wijzigen
        var request = new AuthDTO.UpdateBedrijfInfoRequest
        {
            Field = "bedrijfnaam",
            NewValue = "NewNaam"
        };

        // Act: voer de update uit
        var actionResult = await controller.UpdateBedrijfInfo(request);

        // Assert: verwacht Ok en controleer of de bedrijfsnaam daadwerkelijk is gewijzigd in de DB
        actionResult.Should().BeOfType<OkObjectResult>();
        var updated = context.Bedrijven.FirstOrDefault(b => b.KVK == kvk);
        updated.Should().NotBeNull();
        updated.BedrijfNaam.Should().Be("NewNaam");
    }
}
