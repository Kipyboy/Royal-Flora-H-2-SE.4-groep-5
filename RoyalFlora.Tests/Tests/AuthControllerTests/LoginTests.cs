
using RoyalFlora.Controllers;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using RoyalFlora.Tests.Helpers;



namespace RoyalFlora.Tests.Tests.AuthControllerTests;
 
 public class LoginTests {



    [Fact]
    public async Task Login_ReturnsBadRequest_WhenMissingReqFields () {

        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

        var request = new AuthDTO.LoginRequest
        {
            Email = "",
            Password = "test123!"
        };
        var actionResult = await controller.Login(request);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
     }
     [Fact]
     public async Task Login_ReturnsUnauthorized_WhenIncorrectPassword ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.LoginRequest
        {
            Email = "test@gmail.com",
             Password = "password321?"
        };
            var actionResult = await controller.Login(request);

            actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenIncorrectEmail () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.LoginRequest {
            Email = "incorrect@gmail.com",
            Password = "test123!"
        };

        var actionResult = await controller.Login(request);

        actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();

    }
    [Fact]
    public async Task Login_ReturnsOK_WhenEverythingIsCorrect () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.LoginRequest {
            Email = "test@gmail.com",
            Password = "test123!"
        };

        var actionResult = await controller.Login(request);

        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenWhitespacesInInputs () {
        
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.LoginRequest {
            Email = "test@   gmai   l.com",
            Password = "test123!"
        };

        var actionResult = await controller.Login(request);

        actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    [Fact]
    public async Task Login_ReturnsOk_WhenCaseVariationInEmail () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.LoginRequest {
            Email = "Test@Gmail.com",
            Password = "test123!"
        };

        var actionResult = await controller.Login(request);

        actionResult.Result.Should().BeOfType<OkObjectResult>();
    } 
 }