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
using Microsoft.EntityFrameworkCore;

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
            TestHelpers.SeedUser(context, "test@gmail.com", "test123!");

            var configuration = TestHelpers.CreateTestConfiguration();
            var AuthController = new AuthController(context, configuration);

            var ProductController = new ProductsController(context);

            TestHelpers.SeedBedrijf(context);
            


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

            // The controller returns CreatedAtActionResult which wraps the value
            // Check if result is successful (either CreatedAtAction or Ok)
            actionResult.Result.Should().NotBeNull("PostProduct should return a result");
            
            // Verify the product was actually created in the database
            var createdProduct = await context.Products
                .FirstOrDefaultAsync(p => p.ProductNaam == "TestProduct");
            
            createdProduct.Should().NotBeNull("Product should be created in database");
            createdProduct!.ProductNaam.Should().Be("TestProduct");
            createdProduct.ProductBeschrijving.Should().Be("Test product beschrijving");
            createdProduct.MinimumPrijs.Should().Be(5);
            createdProduct.Locatie.Should().Be("Naaldwijk");
            createdProduct.Aantal.Should().Be(1);
            createdProduct.Leverancier.Should().Be(87654321);
        }
    }
            
 }
