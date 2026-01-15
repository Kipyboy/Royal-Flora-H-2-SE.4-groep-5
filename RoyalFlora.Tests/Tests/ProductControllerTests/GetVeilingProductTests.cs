using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

// Klasse met tests voor de GetVeilingProducts-methode van de ProductsController.
// Deze test controleert of veilingproducten (Status = 3) worden opgehaald en correct gemapped naar DTO's.
public class GetVeilingProductTests
{
    [Fact]
    public async Task GetVeilingProducts_ReturnsFilledList_WhenMethodCompletes()
    {
        // Arrange: maak een unieke in-memory database zodat tests elkaar niet beïnvloeden.
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        // Arrange: seed benodigde gegevens (rollen en een testgebruiker) voor de controller.
        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

        // Arrange: creëer een testproduct met Status = 3 (veiling) en vul relevante velden.
        var product = new Product
        {
            IdProduct = 1,
            ProductNaam = "Test Bloem",
            ProductBeschrijving = "Een testproduct voor unittests",
            Aantal = 10,
            MinimumPrijs = 5.00m,
            Datum = DateTime.UtcNow,
            Locatie = "B",
            Leverancier = null,
            Koper = null,
            verkoopPrijs = 12.50m,
            Status = 3
        };

        // Arrange: sla het product op in de in-memory database.
        context.Products.Add(product);
        context.SaveChanges();

        // Arrange: maak de controller met de testcontext.
        var controller = new ProductsController(context);

        // Act: roep de methode aan die veilingproducten moet retourneren.
        var actionResult = await controller.GetVeilingProducts();
        var list = actionResult.Value;

        // Assert: controleer dat er resultaten zijn en dat het eerder toegevoegde product aanwezig is.
        list.Should().NotBeNull();

        // Zoek de DTO die overeenkomt met het toegevoegde product en controleer de id.
        var dto = list!.FirstOrDefault(x => x.id == product.IdProduct);
        dto.Should().NotBeNull();
        dto!.id.Should().Be(product.IdProduct);
    }
}