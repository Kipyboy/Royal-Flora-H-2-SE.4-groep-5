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
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var controller = new AuthController(context, configuration);

            var request = new AuthDTO.LoginRequest
            {
                Email = "test@gmail.com",
                Password = "test123!"
            };

            var actionResult = await controller.Login(request);
            actionResult.Result.Should().BeOfType<OkObjectResult>();
            var okResult = actionResult.Result as OkObjectResult;
            var response = okResult.Value as LoginResponse;

            response.User.Should().NotBeNull();
            response.Token.Should().NotBeNull();

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
            jwt.Should().NotBeNull();

            var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);

            claims.Should().ContainKey(JwtRegisteredClaimNames.Sub);
            claims[JwtRegisteredClaimNames.Sub].Should().Be(response.User.Id.ToString());

            claims.Should().ContainKey(JwtRegisteredClaimNames.Email);
            claims[JwtRegisteredClaimNames.Email].Should().Be(response.User.Email);

            claims.Should().ContainKey(ClaimTypes.Name);
            claims[ClaimTypes.Name].Should().Be(response.User.Username);

            claims.Should().ContainKey(ClaimTypes.Role);
            claims[ClaimTypes.Role].Should().Be(response.User.Role);

            claims.Should().ContainKey("KVK");
            claims["KVK"].Should().Be(response.User.KVK ?? "");

            claims.Should().ContainKey(JwtRegisteredClaimNames.Jti);
            Guid.TryParse(claims[JwtRegisteredClaimNames.Jti], out var _).Should().BeTrue();
    }
    [Fact]
    public async Task Register_HasCorrectInfoInJWT_AfterSuccesfulRegistration ()
    {
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
            var okResult = actionResult.Result as OkObjectResult;
            var response = okResult.Value as RegisterResponse;

            response.User.Should().NotBeNull();
            response.Token.Should().NotBeNull();

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
            jwt.Should().NotBeNull();

            var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);

            claims.Should().ContainKey(JwtRegisteredClaimNames.Sub);
            claims[JwtRegisteredClaimNames.Sub].Should().Be(response.User.Id.ToString());

            claims.Should().ContainKey(JwtRegisteredClaimNames.Email);
            claims[JwtRegisteredClaimNames.Email].Should().Be(response.User.Email);

            claims.Should().ContainKey(ClaimTypes.Name);
            claims[ClaimTypes.Name].Should().Be(response.User.Username);

            claims.Should().ContainKey(ClaimTypes.Role);
            claims[ClaimTypes.Role].Should().Be(response.User.Role);

            claims.Should().ContainKey("KVK");
            claims["KVK"].Should().Be(response.User.KVK ?? "");

            claims.Should().ContainKey(JwtRegisteredClaimNames.Jti);
            Guid.TryParse(claims[JwtRegisteredClaimNames.Jti], out var _).Should().BeTrue();
    }
}