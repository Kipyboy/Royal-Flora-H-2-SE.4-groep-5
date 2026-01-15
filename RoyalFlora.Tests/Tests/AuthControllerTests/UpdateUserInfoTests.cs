using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MySqlX.XDevAPI.Common;
using RoyalFlora.AuthDTO;
using Org.BouncyCastle.Ocsp;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RoyalFlora.Tests.Tests.AuthControllerTests;

// Tests voor de UpdateUserInfo methode in de AuthController.
// Deze tests controleren verschillende scenario's met een in-memory database:
// - geen ingelogde gebruiker -> 401 Unauthorized
// - ingelogde maar niet-bestaande gebruiker -> 404 NotFound
// - ongeldig veld voor update -> 400 BadRequest
// - wachtwoord update -> speciale behandeling (masked response + gehashte opslag)
// - succesvolle update van gebruikersgegevens -> 200 Ok en verificatie van de wijziging
public class UpdateUserInfoTests
{
    [Fact]
    public async Task UpdateUserInfo_ReturnsUnauthorized_WhenUserNotLoggedIn ()
    {
        // Unieke in-memory database per test om bijvangsten tussen tests te voorkomen
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisgegevens (rollen en minimaal één gebruiker)
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Mock configuratie (geen echte config nodig voor deze test)
        var configuration = TestHelpers.CreateTestConfiguration();
        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Simuleer NIET-ingelogde gebruiker (lege ClaimsPrincipal)
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        // Request om een veld van de gebruiker te wijzigen
        var request = new UpdateUserInfoRequest
        {
            Field = "voornaam",
            NewValue = "changeTest"
        };

        // Act: probeer de update uit te voeren
        var actionResult = await controller.UpdateUserInfo(request);

        // Assert: verwacht Unauthorized omdat er geen ingelogde gebruiker is
        actionResult.Should().BeOfType<UnauthorizedObjectResult>();
    }
    [Fact]
    public async Task UpdateUserInfo_ReturnsNotFound_WhenUserNotFound ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisgegevens
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Simuleer een ingelogde user met een ID die niet in de DB aanwezig is
        var userId = 12345;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        var request = new UpdateUserInfoRequest
        {
            Field = "voornaam",
            NewValue = "changeTest"
        };

        // Act: voer de update uit voor een niet-bestaande gebruiker
        var actionResult = await controller.UpdateUserInfo(request);

        // Assert: verwacht NotFound omdat de gebruiker niet bestaat
        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }
    [Fact]
    public async Task UpdateUserInfo_ReturnsBadRequest_WhenInvalidField ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Gebruik een bestaande gebruiker (Id 1 wordt door TestHelpers aangemaakt)
        var userId = 1;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        // Verzoek bevat een ongeldig veld dat niet geüpdatet kan worden
        var request = new UpdateUserInfoRequest
        {
            Field = "dwaduifhijwf",
            NewValue = "changeTest"
        };

        // Act: roep de update aan
        var actionResult = await controller.UpdateUserInfo(request);

        // Assert: verwacht BadRequest vanwege ongeldig veld
        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }
    [Fact]
    public async Task UpdateUserInfo_ReturnsOkWithCorrectMessage_WhenFieldIsPassword ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed data
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Gebruik een bestaande gebruiker
        var userId = 1;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
        };

        // Vervang wachtwoord (controller returned een gemaskeerde nieuwe waarde in response)
        var request = new UpdateUserInfoRequest
        {
            Field = "wachtwoord",
            NewValue = "changeTest"
        };

        // Act: voer de update uit
        var actionResult = await controller.UpdateUserInfo(request);

        // Assert: verwacht Ok met een response-object dat 'field' en gemaskeerde 'newValue' bevat
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;

        // De controller retourneert een anonymous object { message, field, newValue }
        // We gebruiken reflection om properties te lezen omdat het anonymous type intern is
        var valueType = okResult.Value.GetType();
        var fieldProp = valueType.GetProperty("field");
        var newValueProp = valueType.GetProperty("newValue");

        var fieldVal = fieldProp?.GetValue(okResult.Value) as string;
        var newValueVal = newValueProp?.GetValue(okResult.Value) as string;

        fieldVal.Should().Be("wachtwoord");
        newValueVal.Should().Be("***");

        // Controleer dat in de database het wachtwoord gehasht is en overeenkomt met het nieuwe plain wachtwoord
        var updatedUser = context.Gebruikers.FirstOrDefault(u => u.IdGebruiker == userId);
        updatedUser.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify(request.NewValue, updatedUser.Wachtwoord).Should().BeTrue();
    }
    [Fact]
    public async Task UpdateUserInfo_ReturnsOkAndUpdatesUserCorrectly_WhenMethodCompletes () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata en maak een testgebruiker aan
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        // Simuleer ingelogde gebruiker
        var userId = 1;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
        };

        // Verzoek om voornaam te wijzigen
        var request = new UpdateUserInfoRequest
        {
            Field = "voornaam",
            NewValue = "changeTest"
        };

        // Act: voer update uit
        var actionResult = await controller.UpdateUserInfo(request);

        // Assert: verwacht Ok en controleer de gewijzigde waarde in de DB
        actionResult.Should().BeOfType<OkObjectResult>();
        var updatedUser = context.Gebruikers.FirstOrDefault(u => u.IdGebruiker == userId);
        updatedUser.Should().NotBeNull();
        updatedUser.VoorNaam.Should().Be("changeTest");

    }
}