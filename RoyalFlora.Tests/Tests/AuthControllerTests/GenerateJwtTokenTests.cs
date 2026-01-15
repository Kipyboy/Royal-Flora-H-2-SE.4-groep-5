// Deze testklasse controleert of de gegenereerde JWT (JSON Web Token) de juiste claims bevat
// na een succesvolle login en na een succesvolle registratie.
// We gebruiken een in-memory database en helper-methodes om testdata (rollen, gebruikers, bedrijven) te seeden.
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;
using Microsoft.AspNetCore.Identity.Data;
using RoyalFlora.AuthDTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoyalFlora.Tests.Tests.AuthControllerTests;

public class GenerateJwtTokenTests
{
    [Fact]
    public async Task Login_HasCorrectInfoInJWT_AfterSuccesfulLogin ()
    {
        // Maak een unieke in-memory database voor deze test zodat tests elkaar niet beïnvloeden
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed benodigde data: rollen en een testgebruiker
            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            // Haal testconfiguratie op en maak de controller aan
            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            // Bouw de login-aanvraag met credentials van de seeded gebruiker
            var request = new AuthDTO.LoginRequest
            {
                Email = "test@gmail.com",
                Password = "test123!"
            };

            // Act: voer de login uit
            var actionResult = await controller.Login(request);
            // Verwacht dat de controller een Ok-result teruggeeft
            actionResult.Result.Should().BeOfType<OkObjectResult>();
            var okResult = actionResult.Result as OkObjectResult;
            var response = okResult.Value as LoginResponse;

            // Controleer dat er een user-object en token aanwezig zijn in het antwoord
            response.User.Should().NotBeNull();
            response.Token.Should().NotBeNull();

            // Parse de JWT en controleer dat het token correct is gevormd
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
            jwt.Should().NotBeNull();

            // Zet de claims om in een dictionary zodat we ze gemakkelijk kunnen asserten
            var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);

            // Controleer de standaard JWT-claims en dat ze overeenkomen met de gebruikersdata
            claims.Should().ContainKey(JwtRegisteredClaimNames.Sub);
            claims[JwtRegisteredClaimNames.Sub].Should().Be(response.User.Id.ToString());

            claims.Should().ContainKey(JwtRegisteredClaimNames.Email);
            claims[JwtRegisteredClaimNames.Email].Should().Be(response.User.Email);

            claims.Should().ContainKey(ClaimTypes.Name);
            claims[ClaimTypes.Name].Should().Be(response.User.Username);

            claims.Should().ContainKey(ClaimTypes.Role);
            claims[ClaimTypes.Role].Should().Be(response.User.Role);

            // Extra custom claim: KVK (kan leeg zijn)
            claims.Should().ContainKey("KVK");
            claims["KVK"].Should().Be(response.User.KVK ?? "");

            // JTI moet een geldige GUID bevatten (unieke id voor het token)
            claims.Should().ContainKey(JwtRegisteredClaimNames.Jti);
            Guid.TryParse(claims[JwtRegisteredClaimNames.Jti], out var _).Should().BeTrue();
    }

    [Fact]
    public async Task Register_HasCorrectInfoInJWT_AfterSuccesfulRegistration ()
    {
        // Unieke in-memory database voor isolatie
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            // Seed rollen en een gebruiker; seed daarnaast een bedrijf gekoppeld aan die gebruiker
            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");
            TestHelpers.SeedBedrijf(context, gebruiker);

            // Haal testconfiguratie op en maak de controller aan
            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            // Bouw een registratie-aanvraag met testgegevens
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

        // Act: registreer de nieuwe gebruiker
        var actionResult = await controller.Register(request);
            // Verwacht een Ok-result met registratie-antwoord
            actionResult.Result.Should().BeOfType<OkObjectResult>();
            var okResult = actionResult.Result as OkObjectResult;
            var response = okResult.Value as RegisterResponse;

            // Controleer dat er een user-object en token aanwezig zijn in het antwoord
            response.User.Should().NotBeNull();
            response.Token.Should().NotBeNull();

            // Parse de JWT en controleer dat het token correct is gevormd
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
            jwt.Should().NotBeNull();

            // Zet de claims om in een dictionary zodat we ze gemakkelijk kunnen asserten
            var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);

            // Controleer dat de belangrijkste claims aanwezig en correct zijn
            claims.Should().ContainKey(JwtRegisteredClaimNames.Sub);
            claims[JwtRegisteredClaimNames.Sub].Should().Be(response.User.Id.ToString());

            claims.Should().ContainKey(JwtRegisteredClaimNames.Email);
            claims[JwtRegisteredClaimNames.Email].Should().Be(response.User.Email);

            claims.Should().ContainKey(ClaimTypes.Name);
            claims[ClaimTypes.Name].Should().Be(response.User.Username);

            claims.Should().ContainKey(ClaimTypes.Role);
            claims[ClaimTypes.Role].Should().Be(response.User.Role);

            // Extra custom claim: KVK (kan leeg zijn)
            claims.Should().ContainKey("KVK");
            claims["KVK"].Should().Be(response.User.KVK ?? "");

            // JTI controle: moet een geldige GUID zijn
            claims.Should().ContainKey(JwtRegisteredClaimNames.Jti);
            Guid.TryParse(claims[JwtRegisteredClaimNames.Jti], out var _).Should().BeTrue();
    }
}