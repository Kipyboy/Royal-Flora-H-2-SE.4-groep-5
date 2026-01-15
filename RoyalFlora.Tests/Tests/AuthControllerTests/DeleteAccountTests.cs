// Deze testklasse bevat unit tests voor de DeleteAccount actie van de AuthController.
// De tests gebruiken een in-memory database zodat elke test geïsoleerd en deterministisch is.
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RoyalFlora.Tests.Tests.AuthControllerTests;

public class DeleteAccountTests
{
    [Fact]
    public async Task DeleteAccount_ReturnsUnauthorized_WhenNoNameClaim ()
    {
        // Unieke naam voor de in-memory database zodat tests geen data delen
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed standaard data: rollen en een gebruiker in de testdatabase
            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Mocken van configuratie en aanmaken van de controller
            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            // Simuleren van een request zonder NameIdentifier-claim (anonieme gebruiker)
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
                };

                // Act: probeer het account te verwijderen
                var actionResult = await controller.DeleteAccount();

                // Assert: verwacht Unauthorized als er geen gebruiker in de claims staat
                actionResult.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task DeleteAccount_ReturnsNotFound_WhenNoUserAssociatedWithClaim ()
    {
        // Unieke naam voor de in-memory database zodat tests geen data delen
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed standaard data: rollen en een gebruiker in de testdatabase
            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Mocken van configuratie en aanmaken van de controller
            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            // Gebruik een userId die niet bestaat in de testdatabase
            var userId = 12345; // niet seeded in DB
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            // Simuleer een request met een NameIdentifier-claim die naar een niet-bestaande gebruiker verwijst
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
                };

            // Act: probeer het account te verwijderen
            var actionResult = await controller.DeleteAccount();

            // Assert: verwacht NotFound als de gebruiker uit de claim niet in de database zit
            actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteAccount_ReturnsOK_WhenUserIsFound ()
    {
        // Unieke naam voor de in-memory database zodat tests geen data delen
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed standaard data: rollen en een gebruiker in de testdatabase
            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Mocken van configuratie en aanmaken van de controller
            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            // Gebruik het seeded userId (1) zodat de controller de gebruiker vindt
            var userId = 1;
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            // Simuleer een request met een geldige NameIdentifier-claim
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
                };

            // Act: probeer het account te verwijderen
            var actionResult = await controller.DeleteAccount();

            // Assert: verwacht Ok als de gebruiker gevonden en verwijderd kan worden
            actionResult.Should().BeOfType<OkObjectResult>();
    }
}