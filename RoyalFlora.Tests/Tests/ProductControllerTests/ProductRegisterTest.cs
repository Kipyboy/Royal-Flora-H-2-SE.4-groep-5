using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoyalFlora.Tests.Helpers;
using Xunit;
using RoyalFlora.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using RoyalFlora.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace RoyalFlora.Tests.Tests.ProductControllerTests
{
    public class ProductRegisterTest
    {
        [Fact]
        public async Task PostProduct_ReturnCreatedAtAction_WhenObjectIsInserted()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = TestHelpers.CreateInMemoryContext(dbName);

            TestHelpers.SeedRollen(context);
            Gebruiker gebruiker = TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var AuthController = new AuthController(context, configuration);

            var ProductController = new ProductsController(context);

            TestHelpers.SeedBedrijf(context, gebruiker);
            


            var actionResult = await ProductController.PostProduct(
                ProductNaam: "TestProduct",
                ProductBeschrijving: "Test product beschrijving",
                MinimumPrijs: "5",
                Locatie: "Naaldwijk",
                Datum: "2025-12-20",
                Aantal: "1",
                Leverancier: "87654321",
                images: new List<IFormFile>()
            );

            var createdAtResult = actionResult.Result as CreatedAtActionResult;
            var response = createdAtResult.Value as ResponseDTO;

            response.naam.Should().BeSameAs("TestProduct");
        }
    }
            
 }
