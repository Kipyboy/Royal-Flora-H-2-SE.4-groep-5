using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using RoyalFlora.Controllers;
using RoyalFlora.Tests.Helpers;

public class GetVeilingProductTests
{
    [Fact]
    public async Task GetVeilingProducts_ReturnsFilledList_WhenMethodCompletes()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = TestHelpers.CreateInMemoryContext(dbName);

        TestHelpers.SeedRollen(context);
        TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

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

        context.Products.Add(product);
        context.SaveChanges();

        var controller = new ProductsController(context);

        var actionResult = await controller.GetVeilingProducts();
        var list = actionResult.Value;

        list.Should().NotBeNull();

        var dto = list!.FirstOrDefault(x => x.id == product.IdProduct);
        dto.Should().NotBeNull();
        dto!.id.Should().Be(product.IdProduct);
    }
}