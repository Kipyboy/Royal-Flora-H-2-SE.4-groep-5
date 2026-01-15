// Unit tests voor de KvkExists actie van de AuthController.
// Deze tests valideren de invoer van KVK-nummers en controleren of een KVK nummer al in gebruik is.
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
        // Maak een unieke in-memory database voor isolatie van tests
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed rollen en een gebruiker als basisdata
            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Maak de controller aan met testconfiguratie
            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            // Seed een bedrijf (zodat er bestaande KVKs aanwezig zijn voor sommige tests)
            TestHelpers.SeedBedrijf(context, gebruiker);

            // Ongeldig leeg KVK-nummer
            string kvk = "";

            // Act: roep de KvkExists actie aan
            var actionResult = await controller.KvkExists(kvk);

            // Assert: verwacht BadRequest voor lege invoer
            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task KvKExists_ReturnsBadRequest_WhenKvKIncorrectLength ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed basisdata
            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Maak controller
            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            // Seed bedrijf
            TestHelpers.SeedBedrijf(context, gebruiker);

            // KVK met verkeerde lengte (9 tekens i.p.v. 8)
            string kvk = "123456789";

            // Act
            var actionResult = await controller.KvkExists(kvk);

            // Assert: verwacht BadRequest bij onjuiste lengte
            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Fact]
    public async Task KvKExists_ReturnsBadRequest_WhenNonNumericKvKGiven ()
    {
       // Unieke in-memory database
       var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed basisdata
            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Maak controller
            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            // Seed bedrijf
            TestHelpers.SeedBedrijf(context, gebruiker);

            // KVK met niet-numerieke karakters
            string kvk = "123456AB";

            // Act
            var actionResult = await controller.KvkExists(kvk);

            // Assert: verwacht BadRequest bij niet-numerieke invoer
            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Fact]
    public async Task KvKExists_ReturnsOk_WhenCorrectKvK ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed basisdata
            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Maak controller
            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            // Seed bedrijf (hier zit o.a. KVK 87654321 in)
            TestHelpers.SeedBedrijf(context, gebruiker);

            // Correct KVK-nummer dat reeds bestaat
            string kvk = "87654321";

            // Act
            var actionResult = await controller.KvkExists(kvk);

            // Assert: verwacht Ok(true) wanneer KVK reeds bestaat
            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull("Expected OkObjectResult but got null.");
            var passed = (bool)okResult.Value;
            passed.Should().BeTrue();
    }

    [Fact]
    public async Task KvKExists_ReturnsOk_WhenKvKNotInBedrijf ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed basisdata
            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Maak controller
            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            // Seed bedrijf
            TestHelpers.SeedBedrijf(context, gebruiker);

            // KVK-nummer dat niet in de seeded bedrijven voorkomt
            string kvk = "87654331";

            // Act
            var actionResult = await controller.KvkExists(kvk);

            // Assert: verwacht Ok(false) wanneer KVK niet gevonden is
            var okResult = actionResult.Result as OkObjectResult;
            okResult.Should().NotBeNull("Expected OkObjectResult but got null.");
            var passed = (bool)okResult.Value;
            passed.Should().BeFalse();
    }
}