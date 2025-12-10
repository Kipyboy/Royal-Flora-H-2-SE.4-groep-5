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
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
                };

                var actionResult = await controller.DeleteAccount();

                actionResult.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task DeleteAccount_ReturnsNotFound_WhenNoUserAssociatedWithClaim ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            var userId = 12345; // not seeded in DB
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
                };

            var actionResult = await controller.DeleteAccount();

            actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteAccount_ReturnsOK_WhenUserIsFound ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            var userId = 1;
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
                };

            var actionResult = await controller.DeleteAccount();

            actionResult.Should().BeOfType<OkObjectResult>();
    }
}