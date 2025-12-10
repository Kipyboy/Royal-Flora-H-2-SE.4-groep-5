using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using RoyalFlora.Controllers;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using FluentAssertions;
using RoyalFlora.Tests.Helpers;
using Microsoft.AspNetCore.Identity.Data;
using Org.BouncyCastle.Ocsp;
using Azure;
using RoyalFlora.AuthDTO;

namespace RoyalFlora.Tests.Tests.AuthControllerTests;

public class RegisterTests
{
    [Fact]
    public async Task Register_returnsBadRequest_WhenMissingReqField ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            Telefoonnummer = "0612345678",
            E_mail = "test@gmail.nl",
            Wachtwoord = "test123",
            Postcode = "1234AB",
            Adres = "Straat 1",
        };
        var actionResult = await controller.Register(request);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = actionResult.Result as BadRequestObjectResult;
        var response = result.Value as RegisterResponse;
        response.Message.Should().BeSameAs("Alle velden zijn verplicht");
    }
    [Fact]
    public async Task Register_ReturnsBadRequest_WhenDuplicateEmail ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            AchterNaam = "van der Test",
            Telefoonnummer = "0612345678",
            E_mail = "test@gmail.com",
            Wachtwoord = "test123",
            KvkNummer = "87654321",
            AccountType = "Aanvoerder",
            Postcode = "5678CD",
            Adres = "Kerkstraat 5",
            BedrijfNaam = "Test BV",
            BedrijfPostcode = "5678CD",
            BedrijfAdres = "Bedrijfsstraat 10",
        };

        var actionResult = await controller.Register(request);
        
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = actionResult.Result as BadRequestObjectResult;
        var response = result.Value as RegisterResponse;
        response.Message.Should().BeSameAs("Email adres is al in gebruik");
    }

    [Fact]
    public async Task Register_ReturnsUnauthorized_WhenKvKLessThan8Numbers () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            AchterNaam = "van der Test",
            Telefoonnummer = "0612345678",
            E_mail = "test@gmail.nl",
            Wachtwoord = "test123",
            KvkNummer = "876543",
            AccountType = "Aanvoerder",
            Postcode = "5678CD",
            Adres = "Kerkstraat 5",
            BedrijfNaam = "Test BV",
            BedrijfPostcode = "5678CD",
            BedrijfAdres = "Bedrijfsstraat 10",
        };

        var actionResult = await controller.Register(request);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = actionResult.Result as BadRequestObjectResult;
        var response = result.Value as RegisterResponse;
        response.Message.Should().BeSameAs("KvK-nummer moet 8 cijfers bevatten");
    }
    [Fact]
    public async Task Register_OprichterIsSetCorrectly_WhenNewBedrijfRegistered ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            AchterNaam = "van der Test",
            Telefoonnummer = "0612345678",
            E_mail = "newuser@example.com",
            Wachtwoord = "test123",
            KvkNummer = "87654321",
            AccountType = "Aanvoerder",
            Postcode = "5678CD",
            Adres = "Kerkstraat 5",
            BedrijfNaam = "Test BV",
            BedrijfPostcode = "5678CD",
            BedrijfAdres = "Bedrijfsstraat 10",
        };

        var actionResult = await controller.Register(request);

        
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult.Result as OkObjectResult;
        okResult.Value.Should().BeOfType<AuthDTO.RegisterResponse>();
        var response = okResult.Value as AuthDTO.RegisterResponse;

        response.Success.Should().BeTrue();
        response.User.Should().NotBeNull();

        var createdUserId = response.User.Id;

        int kvkNum = int.Parse(request.KvkNummer);
        var bedrijf = await context.Bedrijven.SingleOrDefaultAsync(b => b.KVK == kvkNum);
        bedrijf.Should().NotBeNull();
        bedrijf.Oprichter.Should().Be(createdUserId);
    }

    [Fact]
    public async Task Register_Completes_WhenNoBedrijfRegistration () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
        TestHelpers.SeedBedrijf(context, gebruiker);

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            AchterNaam = "van der Test",
            Telefoonnummer = "0612345678",
            E_mail = "newuser@example.com",
            Wachtwoord = "test123",
            KvkNummer = "87654321",
            AccountType = "Aanvoerder",
            Postcode = "5678CD",
            Adres = "Kerkstraat 5",
        };

        var actionResult = await controller.Register(request);

        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task Register_Completes_WhenBedrijfRegistration () {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
    
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);
        

        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            AchterNaam = "van der Test",
            Telefoonnummer = "0612345678",
            E_mail = "newuser@example.com",
            Wachtwoord = "test123",
            KvkNummer = "87654321",
            AccountType = "Aanvoerder",
            Postcode = "5678CD",
            Adres = "Kerkstraat 5",
            BedrijfNaam = "Test BV",
            BedrijfPostcode = "5678CD",
            BedrijfAdres = "Bedrijfsstraat 10",
        };

        var actionResult = await controller.Register(request);

        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task Register_Rejects_KVKWithLetters ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
    
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);
        

        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            AchterNaam = "van der Test",
            Telefoonnummer = "0612345678",
            E_mail = "newuser@example.com",
            Wachtwoord = "test123",
            KvkNummer = "876543AB",
            AccountType = "Aanvoerder",
            Postcode = "5678CD",
            Adres = "Kerkstraat 5",
            BedrijfNaam = "Test BV",
            BedrijfPostcode = "5678CD",
            BedrijfAdres = "Bedrijfsstraat 10",
        };

        var actionResult = await controller.Register(request);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        var BadRequest = actionResult.Result as BadRequestObjectResult;

        var response = BadRequest.Value as AuthDTO.RegisterResponse;
        
        var message = response.Message;
        message.Should().BeSameAs("KvK-nummer moet 8 cijfers bevatten");
    }
    [Fact]
    public async Task Register_SetsAanvoerderRoleCorrectly ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
    
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);
        

        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            AchterNaam = "van der Test",
            Telefoonnummer = "0612345678",
            E_mail = "newuser@example.com",
            Wachtwoord = "test123",
            KvkNummer = "87654321",
            AccountType = "Aanvoerder",
            Postcode = "5678CD",
            Adres = "Kerkstraat 5",
            BedrijfNaam = "Test BV",
            BedrijfPostcode = "5678CD",
            BedrijfAdres = "Bedrijfsstraat 10",
        };

        var actionResult = await controller.Register(request);

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult.Value as AuthDTO.RegisterResponse;

        response.User.Role.Should().BeSameAs("Aanvoerder");

        
    }
    [Fact]
    public async Task Register_SetsInkoperRoleCorrectly ()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
    
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);
        

        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            AchterNaam = "van der Test",
            Telefoonnummer = "0612345678",
            E_mail = "newuser@example.com",
            Wachtwoord = "test123",
            KvkNummer = "87654321",
            AccountType = "Inkoper",
            Postcode = "5678CD",
            Adres = "Kerkstraat 5",
            BedrijfNaam = "Test BV",
            BedrijfPostcode = "5678CD",
            BedrijfAdres = "Bedrijfsstraat 10",
        };

        var actionResult = await controller.Register(request);

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult.Value as AuthDTO.RegisterResponse;

        response.User.Role.Should().BeSameAs("Inkoper");
    }

 }