using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RoyalFlora.Tests.Tests.AuthControllerTests;

public class UpdateBedrijfInfoTests
{
    [Fact]
    public async Task UpdateBedrijfInfo_ReturnsUnauthorized_WhenUserNotLoggedIn()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        var request = new AuthDTO.UpdateBedrijfInfoRequest
        {
            Field = "bedrijfnaam",
            NewValue = "changeTest"
        };

        var actionResult = await controller.UpdateBedrijfInfo(request);

        actionResult.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdateBedrijfInfo_ReturnsNotFound_WhenUserNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

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

        var actionResult = await controller.UpdateBedrijfInfo(request);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateBedrijfInfo_ReturnsBadRequest_WhenInvalidField()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        var userId = 1;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        var request = new AuthDTO.UpdateBedrijfInfoRequest
        {
            Field = "invalidfield",
            NewValue = "changeTest"
        };

        var actionResult = await controller.UpdateBedrijfInfo(request);

        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateBedrijfInfo_ReturnsOkAndUpdatesBedrijfCorrectly_WhenMethodCompletes()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        var user = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Create company and link to user
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
        user.KVK = kvk;
        context.Gebruikers.Update(user);
        await context.SaveChangesAsync();

        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        var userId = user.IdGebruiker;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        var request = new AuthDTO.UpdateBedrijfInfoRequest
        {
            Field = "bedrijfnaam",
            NewValue = "NewNaam"
        };

        var actionResult = await controller.UpdateBedrijfInfo(request);

        actionResult.Should().BeOfType<OkObjectResult>();
        var updated = context.Bedrijven.FirstOrDefault(b => b.KVK == kvk);
        updated.Should().NotBeNull();
        updated.BedrijfNaam.Should().Be("NewNaam");
    }
}
