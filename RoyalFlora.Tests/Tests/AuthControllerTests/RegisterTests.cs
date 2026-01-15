// Unit tests voor de Register actie van de AuthController.
// De tests verifiëren validatie, rol-toewijzing en het aanmaken van bedrijven/gebruikers.
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
        // Unieke in-memory database voor isolatie
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed benodigde data: rollen en een bestaande gebruiker
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Maak controller met testconfiguratie
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Bouw een registratie-request met een missend verplicht veld
        var request = new AuthDTO.RegisterRequest
        {
            VoorNaam = "Test",
            Telefoonnummer = "0612345678",
            E_mail = "test@gmail.nl",
            Wachtwoord = "test123",
            Postcode = "1234AB",
            Adres = "Straat 1",
        };

        // Act: probeer te registreren
        var actionResult = await controller.Register(request);

        // Assert: verwacht BadRequest met specifieke foutmelding
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = actionResult.Result as BadRequestObjectResult;
        var response = result.Value as RegisterResponse;
        response.Message.Should().BeSameAs("Alle velden zijn verplicht");
    }
    [Fact]
    public async Task Register_ReturnsBadRequest_WhenDuplicateEmail ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata (er bestaat al een gebruiker met hetzelfde e-mailadres)
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Registratie-request met duplicate e-mail
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

        // Act
        var actionResult = await controller.Register(request);
        
        // Assert: verwacht BadRequest met melding dat e-mail al in gebruik is
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = actionResult.Result as BadRequestObjectResult;
        var response = result.Value as RegisterResponse;
        response.Message.Should().BeSameAs("Email adres is al in gebruik");
    }

    [Fact]
    public async Task Register_ReturnsUnauthorized_WhenKvKLessThan8Numbers () {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Registratie met een te kort KvK-nummer
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

        // Act
        var actionResult = await controller.Register(request);

        // Assert: verwacht BadRequest met KvK-validatie boodschap
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = actionResult.Result as BadRequestObjectResult;
        var response = result.Value as RegisterResponse;
        response.Message.Should().BeSameAs("KvK-nummer moet 8 cijfers bevatten");
    }
    [Fact]
    public async Task Register_OprichterIsSetCorrectly_WhenNewBedrijfRegistered ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Registratie met bedrijfsgegevens
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

        // Act: registreer nieuwe gebruiker en bedrijf
        var actionResult = await controller.Register(request);

        // Assert: controleer dat registratie is geslaagd en dat het bedrijf de juiste oprichter heeft
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
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata en een bestaand bedrijf
        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
        TestHelpers.SeedBedrijf(context, gebruiker);

        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);

        // Registratie zonder bedrijf
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

        // Act
        var actionResult = await controller.Register(request);

        // Assert: registratie voltooit zonder bedrijfsregistratie
        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task Register_Completes_WhenBedrijfRegistration () {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
    
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);
        

        // Registratie inclusief bedrijfsgegevens
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

        // Act
        var actionResult = await controller.Register(request);

        // Assert: verwacht succesvolle registratie
        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task Register_Rejects_KVKWithLetters ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
    
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);
        

        // Registratie met KvK die letters bevat
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

        // Act
        var actionResult = await controller.Register(request);

        // Assert: verwacht BadRequest met KvK-validatiebericht
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        var BadRequest = actionResult.Result as BadRequestObjectResult;

        var response = BadRequest.Value as AuthDTO.RegisterResponse;
        
        var message = response.Message;
        message.Should().BeSameAs("KvK-nummer moet 8 cijfers bevatten");
    }
    [Fact]
    public async Task Register_SetsAanvoerderRoleCorrectly ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
    
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);
        

        // Registratie met AccountType Aanvoerder
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

        // Act
        var actionResult = await controller.Register(request);

        // Assert: controleer of de juiste rol is toegewezen
        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult.Value as AuthDTO.RegisterResponse;

        response.User.Role.Should().BeSameAs("Aanvoerder");

        
    }
    [Fact]
    public async Task Register_SetsInkoperRoleCorrectly ()
    {
        // Unieke in-memory database
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Seed basisdata
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
    
        var configuration = TestHelpers.CreateTestConfiguration();
        var controller = new AuthController(context, configuration);
        

        // Registratie met AccountType Inkoper
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

        // Act
        var actionResult = await controller.Register(request);

        // Assert: controleer of de juiste rol is toegewezen
        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult.Value as AuthDTO.RegisterResponse;

        response.User.Role.Should().BeSameAs("Inkoper");
    }

 }