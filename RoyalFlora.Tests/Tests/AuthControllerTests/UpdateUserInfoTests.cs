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

public class UpdateUserInfoTests
{
    [Fact]
    public async Task UpdateUserInfo_ReturnsUnauthorized_WhenUserNotLoggedIn ()
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
            var request = new UpdateUserInfoRequest
            {
                Field = "voornaam",
                NewValue = "changeTest"
            };
            
            var actionResult = await controller.UpdateUserInfo(request);

            actionResult.Should().BeOfType<UnauthorizedObjectResult>();
    }
    [Fact]
    public async Task UpdateUserInfo_ReturnsNotFound_WhenUserNotFound ()
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
            var request = new UpdateUserInfoRequest
            {
                Field = "voornaam",
                NewValue = "changeTest"
            };

            var actionResult = await controller.UpdateUserInfo(request);

            actionResult.Should().BeOfType<NotFoundObjectResult>();
    }
    [Fact]
    public async Task UpdateUserInfo_ReturnsBadRequest_WhenInvalidField ()
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
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
                };
            var request = new UpdateUserInfoRequest
            {
                Field = "dwaduifhijwf",
                NewValue = "changeTest"
            };

            var actionResult = await controller.UpdateUserInfo(request);

            actionResult.Should().BeOfType<BadRequestObjectResult>();
    }
    [Fact]
    public async Task UpdateUserInfo_ReturnsOkWithCorrectMessage_WhenFieldIsPassword ()
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
            var request = new UpdateUserInfoRequest
            {
                Field = "wachtwoord",
                NewValue = "changeTest"
            };

            var actionResult = await controller.UpdateUserInfo(request);

            actionResult.Should().BeOfType<OkObjectResult>();
            var okResult = actionResult as OkObjectResult;
            // The controller returns an anonymous object: { message, field, newValue }
            // Use reflection to read properties because the anonymous type is internal to the controller assembly
            var valueType = okResult.Value.GetType();
            var fieldProp = valueType.GetProperty("field");
            var newValueProp = valueType.GetProperty("newValue");

            var fieldVal = fieldProp?.GetValue(okResult.Value) as string;
            var newValueVal = newValueProp?.GetValue(okResult.Value) as string;

            fieldVal.Should().Be("wachtwoord");
            newValueVal.Should().Be("***");

            // Verify the database stored a hashed password that matches the new plain password
            var updatedUser = context.Gebruikers.FirstOrDefault(u => u.IdGebruiker == userId);
            updatedUser.Should().NotBeNull();
            BCrypt.Net.BCrypt.Verify(request.NewValue, updatedUser.Wachtwoord).Should().BeTrue();
    }
    [Fact]
    public async Task UpdateUserInfo_ReturnsOkAndUpdatesUserCorrectly_WhenMethodCompletes () {
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
            var request = new UpdateUserInfoRequest
            {
                Field = "voornaam",
                NewValue = "changeTest"
            };

            var actionResult = await controller.UpdateUserInfo(request);

            actionResult.Should().BeOfType<OkObjectResult>();
            var updatedUser = context.Gebruikers.FirstOrDefault(u => u.IdGebruiker == userId);
            updatedUser.Should().NotBeNull();
            updatedUser.VoorNaam.Should().Be("changeTest");

    }
}