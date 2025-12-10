using FluentAssertions;
using RoyalFlora.Tests.Helpers;
using RoyalFlora.Controllers;
using Microsoft.AspNetCore.Mvc;


namespace RoyalFlora.Tests.Tests.AuthControllerTests;

public class KvkExistsTests
{
    [Fact]
    public async Task KvKExists_ReturnsBadRequest_WhenNoKvKGiven ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            TestHelpers.SeedBedrijf(context, gebruiker);

            string kvk = "";

            var actionResult = await controller.KvkExists(kvk);

            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task KvKExists_ReturnsBadRequest_WhenKvKIncorrectLength ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            TestHelpers.SeedBedrijf(context, gebruiker);

            string kvk = "123456789";

            var actionResult = await controller.KvkExists(kvk);

            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Fact]
    public async Task KvKExists_ReturnsBadRequest_WhenNonNumericKvKGiven ()
    {
       var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            TestHelpers.SeedBedrijf(context, gebruiker);

            string kvk = "123456AB";

            var actionResult = await controller.KvkExists(kvk);

            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Fact]
    public async Task KvKExists_ReturnsOk_WhenCorrectKvK ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            TestHelpers.SeedBedrijf(context, gebruiker);

            string kvk = "87654321";

            var actionResult = await controller.KvkExists(kvk);

            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull("Expected OkObjectResult but got null.");
            var passed = (bool)okResult.Value;
            passed.Should().BeTrue();
    }

    [Fact]
    public async Task KvKExists_ReturnsOk_WhenKvKNotInBedrijf ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            TestHelpers.SeedBedrijf(context, gebruiker);

            string kvk = "87654331";

            var actionResult = await controller.KvkExists(kvk);

            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull("Expected OkObjectResult but got null.");
            var passed = (bool)okResult.Value;
            passed.Should().BeFalse();
    }
}