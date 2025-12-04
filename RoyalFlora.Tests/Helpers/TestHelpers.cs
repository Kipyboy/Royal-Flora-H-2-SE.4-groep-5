using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RoyalFlora.Controllers;

namespace RoyalFlora.Tests.Helpers
{
    public static class TestHelpers
    {
        public static MyDbContext CreateInMemoryContext(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            return new MyDbContext(options);
        }

        public static IConfiguration CreateTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "super-secret-test-key-should-be-long-enough"},
                {"Jwt:Issuer", "test"},
                {"Jwt:Audience", "test"},
                {"Jwt:ExpirationInMinutes", "60"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        public static AuthController CreateAuthController(MyDbContext context, IConfiguration? configuration = null)
        {
            return new AuthController(context, configuration ?? CreateTestConfiguration());
        }

        public static Gebruiker SeedUser(MyDbContext context, string email, string plainPassword, int roleId = 2)
        {
            // Ensure role exists
            var role = context.Rollen.Find(roleId);
            if (role == null)
            {
                role = new Rol { IdRollen = roleId, RolNaam = roleId == 1 ? "Aanvoerder" : "Inkoper" };
                context.Rollen.Add(role);
            }

            var hashed = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            var gebruiker = new Gebruiker
            {
                VoorNaam = "Test",
                AchterNaam = "User",
                Email = email,
                Wachtwoord = hashed,
                Rol = roleId,
                RolNavigation = role
            };

            context.Gebruikers.Add(gebruiker);
            context.SaveChanges();

            return gebruiker;
        }
        public static void SeedRol (MyDbContext context)
        {
            var role = new Rol { IdRollen = 2, RolNaam = "Inkoper" };
            context.Rollen.Add(role);
            context.SaveChanges();
        }
    }
}
