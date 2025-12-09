using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using RoyalFlora.Controllers;
using System.Reflection;
using RoyalFlora.Tests.Helpers;

namespace RoyalFlora.Tests.Tests.ProductControllerTests
{
    public class VeilingProductInLadenTest
    {
        /// <summary>
        /// Test 1: Test with a complete Product that has all values filled in
        /// This is the "happy path" - everything is provided
        /// </summary>
        [Fact]
        public void VeilingProductInLaden_WithCompleteProduct_ReturnsVeilingDTO()
        {
            // ARRANGE - Set up the test data
            // Create an in-memory database using the helper
            var context = TestHelpers.CreateInMemoryContext();
            
            // Create a sample Product with all values
            var product = new Product
            {
                IdProduct = 1,
                ProductNaam = "Tulips",
                ProductBeschrijving = "Beautiful red tulips",
                Locatie = "Naaldwijk",
                Status = 3
            };

            // Create an instance of ProductsController with the in-memory context
            var controller = new ProductsController(context);

            // ACT - Execute the method we're testing using reflection
            var method = typeof(ProductsController).GetMethod(
                "VeilingProductInLaden",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            var result = method?.Invoke(controller, new object[] { product }) as VeilingDTO;

            // ASSERT - Verify the results are correct
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Tulips", result.naam);
            Assert.Equal("Beautiful red tulips", result.beschrijving);
            Assert.Equal("Naaldwijk", result.locatie);
            Assert.Equal(3, result.status);
        }

        /// <summary>
        /// Test 2: Test with null values to ensure they are converted to defaults
        /// This tests the null-coalescing operator (??)
        /// </summary>
        [Fact]
        public void VeilingProductInLaden_WithNullValues_ReturnsDefaultValues()
        {
            // ARRANGE - Create an in-memory database
            var context = TestHelpers.CreateInMemoryContext();
            
            // Create a Product with null values
            var product = new Product
            {
                IdProduct = 2,
                ProductNaam = null,              // null should become empty string
                ProductBeschrijving = null,      // null should become empty string
                Locatie = null,                  // null should become empty string
                Status = null                    // null should become 0
            };

            var controller = new ProductsController(context);

            // ACT - Call the method using reflection
            var method = typeof(ProductsController).GetMethod(
                "VeilingProductInLaden",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            var result = method?.Invoke(controller, new object[] { product }) as VeilingDTO;

            // ASSERT - Verify nulls are replaced with defaults
            Assert.NotNull(result);
            Assert.Equal(2, result.id);
            Assert.Equal(string.Empty, result.naam);           // Should be empty string, not null
            Assert.Equal(string.Empty, result.beschrijving);   // Should be empty string, not null
            Assert.Equal(string.Empty, result.locatie);        // Should be empty string, not null
            Assert.Equal(0, result.status);                    // Should be 0, not null
        }

        /// <summary>
        /// Test 3: Test with mixed null and non-null values
        /// </summary>
        [Fact]
        public void VeilingProductInLaden_WithMixedValues_ReturnsMixedResult()
        {
            // ARRANGE
            var context = TestHelpers.CreateInMemoryContext();
            
            var product = new Product
            {
                IdProduct = 3,
                ProductNaam = "Roses",
                ProductBeschrijving = null,     // This is null
                Locatie = "Aalsmeer",
                Status = 2
            };

            var controller = new ProductsController(context);

            // ACT
            var method = typeof(ProductsController).GetMethod(
                "VeilingProductInLaden",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            var result = method?.Invoke(controller, new object[] { product }) as VeilingDTO;

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("Roses", result.naam);                // Has value
            Assert.Equal(string.Empty, result.beschrijving);   // Null converted to empty
            Assert.Equal("Aalsmeer", result.locatie);          // Has value
        }

        /// <summary>
        /// Test 4: Test with Status = 0 (which is already the default, but let's verify)
        /// </summary>
        [Fact]
        public void VeilingProductInLaden_WithZeroStatus_ReturnsZero()
        {
            // ARRANGE
            var context = TestHelpers.CreateInMemoryContext();
            
            var product = new Product
            {
                IdProduct = 4,
                ProductNaam = "Daisy",
                ProductBeschrijving = "White daisies",
                Locatie = "Rijnsburg",
                Status = 0
            };

            var controller = new ProductsController(context);

            // ACT
            var method = typeof(ProductsController).GetMethod(
                "VeilingProductInLaden",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            var result = method?.Invoke(controller, new object[] { product }) as VeilingDTO;

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
        }
    }
}