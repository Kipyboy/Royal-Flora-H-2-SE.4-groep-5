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

        var actionResult = await controller.GetBedrijfInfo();

        actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetBedrijfInfo_ReturnsNotFound_WhenUserNotFound()
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

        var actionResult = await controller.GetBedrijfInfo();

        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetBedrijfInfo_ReturnsOk_WhenBedrijfIsFound()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        var user = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Create a company and link it to the user via KVK
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

        var configMock = new Mock<IConfiguration>();
        var controller = new AuthController(context, configMock.Object);

        var userId = user.IdGebruiker;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "testAuth")) }
        };

        var actionResult = await controller.GetBedrijfInfo();

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
