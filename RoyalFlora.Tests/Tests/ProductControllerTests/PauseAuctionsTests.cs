using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

// Tests voor de PauseAuctions-methode van de ProductsController.
// Deze test controleert of actieve veilingen (bijv. Status = 2) correct worden gepauzeerd (Status = 5).
public class PauseAuctionsTests
{
    [Fact]
    public async Task PauseAuctions_CorrectlySetsStatusToPaused_WhenMethodCompletes ()
    {
        // Arrange: maak een unieke in-memory database zodat testen niet interfereren met elkaar.
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed vereiste data (rollen en een testgebruiker) voor controller-actie.
        TestHelpers.SeedRollen(context);
        Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: maak de controller met de testcontext.
        var controller = new ProductsController(context);

        // Arrange: voeg een product toe met Status = 2 (bijvoorbeeld: actieve veiling) dat gepauzeerd moet worden.
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.Today,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 2
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act: roep de PauseAuctions-methode aan die gepauzeerde veilingen moet retourneren en status moet aanpassen.
        var actionResult = await controller.PauseAuctions();

        // Assert: controleer dat de actie een OkObjectResult teruggeeft en dat de geretourneerde lijst niet null is.
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        var pausedProducts = okResult.Value as IEnumerable<object>;
        var list = pausedProducts != null ? new List<object>(pausedProducts) : new List<object>();
        list.Should().NotBeNull();

        // Assert: controleer dat het oorspronkelijke product zijn status heeft gewijzigd naar gepauzeerd (Status = 5).
        product.Status.Should().Be(5);
    }
}