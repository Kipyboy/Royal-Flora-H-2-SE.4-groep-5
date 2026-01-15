// Unit tests voor de GetUserInfo actie van de AuthController.
// Deze tests controleren gedrag voor niet-ingelogde gebruikers, niet-bestaande gebruikers,
// en wanneer een bestaande gebruiker succesvol opgehaald wordt.
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MySqlX.XDevAPI.Common;

namespace RoyalFlora.Tests.Tests.AuthControllerTests;

public class GetUserInfoTests
{
    [Fact]
    public async Task GetUserInfo_ReturnsUnauthorized_WhenUserNotLoggedIn ()
    {
        // Maak een unieke in-memory database voor isolatie
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed noodzakelijke data (rollen en een testgebruiker)
            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Maak de controller aan (configuratie gemockt)
            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            // Simuleer een request zonder ingelogde gebruiker (geen claims)
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
                };

                // Act: vraag user info op
                var actionResult = await controller.GetUserInfo();

                // Assert: verwacht Unauthorized wanneer er geen gebruiker in de context staat
                actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    [Fact]
    public async Task GetUserInfo_ReturnsNotFound_WhenUserNotFound ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed basisdata
            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Maak de controller aan
            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            // Gebruik een userId dat niet bestaat in de database
            var userId = 12345;
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

            // Simuleer een request met een NameIdentifier-claim die naar een niet-bestaande gebruiker verwijst
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
                };

            // Act: vraag user info op
            var actionResult = await controller.GetUserInfo();

            // Assert: verwacht NotFound als de gebruiker niet in de DB aanwezig is
            actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }
    [Fact]
    public async Task GetUserInfo_ReturnsOk_WhenUserIsFound ()
    {
        // Unieke in-memory database voor isolatie
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed rollen en een gebruiker
            TestHelpers.SeedRollen(context);
            var user = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Maak de controller aan
            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            // Gebruik het seeded userId (1) en simuleer het request met de juiste claim
            var userId = 1;
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
                };

            // Act: vraag user info op
            var actionResult = await controller.GetUserInfo();

            // Assert: verwacht Ok en dat de teruggegeven gebruiker de juiste velden heeft
            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().BeOfType<OkObjectResult>();
            var resultUser = okResult.Value as Gebruiker;
            resultUser.VoorNaam.Should().Be(user.VoorNaam);
            resultUser.AchterNaam.Should().Be(user.AchterNaam);
            resultUser.Email.Should().Be(user.Email);
            resultUser.Telefoonnummer.Should().Be(user.Telefoonnummer);
            resultUser.Adress.Should().Be(user.Adress);
            resultUser.Postcode.Should().Be(user.Postcode);
    }
}