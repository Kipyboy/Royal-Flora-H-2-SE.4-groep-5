
// Unit tests voor de Login actie van de AuthController.
// De tests verifiëren validatie, foutafhandeling en succesvolle login-gevallen.
using RoyalFlora.Controllers;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using RoyalFlora.Tests.Helpers;


namespace RoyalFlora.Tests.Tests.AuthControllerTests;
 
 public class LoginTests {



    [Fact]
    public async Task Login_ReturnsBadRequest_WhenMissingReqFields () {

        // Unieke in-memory database zodat tests geïsoleerd zijn
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed benodigde data: rollen en een testgebruiker
            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Maak controller met testconfiguratie
            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

        // Bouw een login request met ontbrekend email veld
        var request = new AuthDTO.LoginRequest
        {
            Email = "",
            Password = "test123!"
        };

        // Act: voer login uit
        var actionResult = await controller.Login(request);

        // Assert: verwacht BadRequest en een specifieke foutmelding
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = actionResult.Result as BadRequestObjectResult;
        var response = result.Value as AuthDTO.LoginResponse;
        response.Message.Should().BeSameAs("Email en wachtwoord zijn verplicht");
     }
     [Fact]
     public async Task Login_ReturnsUnauthorized_WhenIncorrectPassword ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed rollen en gebruiker
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Maak controller
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Bouw request met correcte email maar verkeerd wachtwoord
        var request = new AuthDTO.LoginRequest
        {
            Email = "test@gmail.com",
             Password = "password321?"
        };
            var actionResult = await controller.Login(request);

            // Assert: verwacht Unauthorized en foutmelding
            actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
            var result = actionResult.Result as UnauthorizedObjectResult;
            var response = result.Value as AuthDTO.LoginResponse;
            response.Message.Should().BeSameAs("Ongeldige inloggegevens");
    }
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenIncorrectEmail () {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Maak controller
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Bouw request met incorrecte email
        var request = new AuthDTO.LoginRequest {
            Email = "incorrect@gmail.com",
            Password = "test123!"
        };

        var actionResult = await controller.Login(request);

        // Assert: verwacht Unauthorized en foutmelding
        actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var result = actionResult.Result as UnauthorizedObjectResult;
        var response = result.Value as AuthDTO.LoginResponse;
        response.Message.Should().BeSameAs("Ongeldige inloggegevens");

    }
    [Fact]
    public async Task Login_ReturnsOK_WhenEverythingIsCorrect () {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed data
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Maak controller
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Geldige login request
        var request = new AuthDTO.LoginRequest {
            Email = "test@gmail.com",
            Password = "test123!"
        };

        // Act: voer login uit
        var actionResult = await controller.Login(request);

        // Assert: verwacht OK
        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenWhitespacesInInputs () {
        
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed data
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Maak controller
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Request met ongeldige spaties in e-mail
        var request = new AuthDTO.LoginRequest {
            Email = "test@   gmai   l.com",
            Password = "test123!"
        };

        var actionResult = await controller.Login(request);

        // Assert: verwacht Unauthorized en foutmelding
        actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var result = actionResult.Result as UnauthorizedObjectResult;
        var response = result.Value as AuthDTO.LoginResponse;
        response.Message.Should().BeSameAs("Ongeldige inloggegevens");
    }
    [Fact]
    public async Task Login_ReturnsOk_WhenCaseVariationInEmail () {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed data
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Maak controller
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Case-variatie in email (moet nog steeds werken)
        var request = new AuthDTO.LoginRequest {
            Email = "Test@Gmail.com",
            Password = "test123!"
        };

        var actionResult = await controller.Login(request);

        // Assert: verwacht OK ook bij hoofdletter-variatie
        actionResult.Result.Should().BeOfType<OkObjectResult>();
    } 
 }