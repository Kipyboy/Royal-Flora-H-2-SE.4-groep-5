using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

// Klasse met tests voor de methode GetStatus1Products van de ProductsController.
// De tests verifiëren verschillende scenario's rondom producten met status == 1.
public class GetStatus1ProductsTests
{
    [Fact]
    public async Task GetStatus1Products_CompletesWithoutErrors_WhenNoStatus1Products ()
    {
        // Arrange: maak een unieke in-memory database per test zodat testen onafhankelijk zijn.
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed noodzakelijke gegevens (rollen en een testgebruiker) voor de controller.
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: maak de controller aan met de in-memory context.
        var controller = new ProductsController(context);

        // Act: roep de methode aan die alle producten met status == 1 moet teruggeven.
        var actionResult = await controller.GetStatus1Products();

        // Assert: haal de waarde uit het action result en controleer dat de lijst leeg is.
        var list = actionResult.Value;

        list.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStatus1Products_ReturnedListContainsProducts_AfterMethodCompletion ()
    {
        // Arrange: maak een unieke in-memory database voor deze test.
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed de rollen en een testgebruiker zodat benodigde referenties aanwezig zijn.
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: maak de controller met de testcontext.
        var controller = new ProductsController(context);

        // Arrange: creëer twee testproducten met Status = 1 (deze moeten teruggegeven worden).
        var testProduct1 = new Product
        {
            ProductNaam = "Test Product 1",
            MinimumPrijs = 10m,
            Status = 1,
            Locatie = "LocatieA"
        };

        var testProduct2 = new Product
        {
            ProductNaam = "Test Product 2",
            MinimumPrijs = 20m,
            Status = 1,
            Locatie = "LocatieB"
        };

        // Arrange: voeg de testproducten toe aan de in-memory database en sla op.
        context.Products.AddRange(testProduct1, testProduct2);
        context.SaveChanges();

        // Act: roep de methode aan om producten met status 1 op te halen.
        var actionResult = await controller.GetStatus1Products();

        // Assert: controleer dat het resultaat de verwachte producten bevat.
        var list = actionResult.Value;

        list.Should().NotBeNull();
        list.Should().HaveCount(2);
        // Controleer op naam omdat het geretourneerde model waarschijnlijk andere property-namen heeft (zoals 'naam').
        list.Should().Contain(p => p.naam == "Test Product 1");
        list.Should().Contain(p => p.naam == "Test Product 2");
    }
}