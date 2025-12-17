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

                var actionResult = await controller.GetUserInfo();

                actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    [Fact]
    public async Task GetUserInfo_ReturnsNotFound_WhenUserNotFound ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            var userId = 12345;
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
                };

            var actionResult = await controller.GetUserInfo();

            actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }
    [Fact]
    public async Task GetUserInfo_ReturnsOk_WhenUserIsFound ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            var user = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var configMock = new Mock<IConfiguration>();
            var controller = new AuthController(context, configMock.Object);

            var userId = 1;
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

            controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
                };

            var actionResult = await controller.GetUserInfo();

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