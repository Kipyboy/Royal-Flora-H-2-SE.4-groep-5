// Unit tests voor de GetBedrijfInfo actie van de AuthController.
// De tests verifiëren gedrag wanneer de gebruiker niet ingelogd is, wanneer de gebruiker niet bestaat,
// en wanneer er wél een bedrijf gekoppeld is aan de gebruiker.
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RoyalFlora.Tests.Tests.AuthControllerTests;

public class GetBedrijfInfoTests
{
    [Fact]
    public async Task GetBedrijfInfo_ReturnsUnauthorized_WhenUserNotLoggedIn()
    {
        // Maak een unieke in-memory database voor isolatie van tests
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed noodzakelijke data (rollen en een testgebruiker)
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Maak de controller aan (configuratie gemockt)
        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Simuleer een request zonder ingelogde gebruiker (geen claims)
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        // Act: vraag bedrijfsinfo op
        var actionResult = await controller.GetBedrijfInfo();

        // Assert: verwacht Unauthorized wanneer er geen gebruiker is ingelogd
        actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetBedrijfInfo_ReturnsNotFound_WhenUserNotFound()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Gebruik een userId die niet in de database bestaat
        var userId = 12345;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        // Simuleer een request met een NameIdentifier-claim die naar een niet-bestaande gebruiker verwijst
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        // Act: vraag bedrijfsinfo op
        var actionResult = await controller.GetBedrijfInfo();

        // Assert: verwacht NotFound als de gebruiker niet in de DB zit
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetBedrijfInfo_ReturnsOk_WhenBedrijfIsFound()
    {
        // Unieke in-memory database voor isolatie
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed rollen en een gebruiker
        TestHelpers.SeedRollen(context);
        var user = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Maak een bedrijf en koppel het aan de gebruiker via het KVK-nummer
        var kvk = new Random().Next(10000000, 99999999);
        var bedrijf = new Bedrijf
        {
            KVK = kvk,
            BedrijfNaam = "TestBedrijf",
            Adress = "TestAdres",
            Postcode = "1234AB",
            Oprichter = user.IdGebruiker
        };
        context.Bedrijven.Add(bedrijf);
        user.KVK = kvk;
        context.Gebruikers.Update(user);
        await context.SaveChangesAsync();

        // Maak de controller aan
        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Simuleer een request met de NameIdentifier-claim van de seeded gebruiker
        var userId = user.IdGebruiker;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        // Act: vraag bedrijfsinfo op
        var actionResult = await controller.GetBedrijfInfo();

        // Assert: verwachte data komt overeen met het seeded bedrijf
        var okResult = actionResult.Result as OkObjectResult;
        okResult.Should().BeOfType<OkObjectResult>();
        var response = okResult.Value as AuthDTO.GetBedrijfInfoResponse;
        response.Should().NotBeNull();
        response.BedrijfNaam.Should().Be(bedrijf.BedrijfNaam);
        response.Postcode.Should().Be(bedrijf.Postcode);
        response.Adres.Should().Be(bedrijf.Adress);
        response.Oprichter.Should().Be(user.VoorNaam);
        response.IsOprichter.Should().BeTrue();
    }
}
