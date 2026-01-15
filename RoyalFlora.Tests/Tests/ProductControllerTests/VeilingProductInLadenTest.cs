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
        /// Test 1: Test met een volledig Product waarin alle velden ingevuld zijn.
        /// Dit is het 'happy path' — alles is aanwezig.
        /// </summary>
        [Fact]
        public void VeilingProductInLaden_WithCompleteProduct_ReturnsVeilingDTO()
        {
            // ARRANGE - Testdata opzetten
            // Maak een in-memory database met behulp van de helper
            var context = TestHelpers.CreateInMemoryContext();
            
            // Maak een voorbeeldproduct met alle benodigde velden
            var product = new Product
            {
                IdProduct = 1,
                ProductNaam = "Tulips",
                ProductBeschrijving = "Beautiful red tulips",
                Locatie = "Naaldwijk",
                Status = 3
            };

            // Maak een ProductsController instantie met de test-context
            var controller = new ProductsController(context);

            // ACT - Voer de te testen methode uit met reflection
            var method = typeof(ProductsController).GetMethod(
                "VeilingProductInLaden",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            var result = method?.Invoke(controller, new object[] { product }) as VeilingDTO;

            // ASSERT - Verifieer dat de output overeenkomt met het input product
            Assert.NotNull(result);
            Assert.Equal(1, result.id);
            Assert.Equal("Tulips", result.naam);
            Assert.Equal("Beautiful red tulips", result.beschrijving);
            Assert.Equal("Naaldwijk", result.locatie);
            Assert.Equal(3, result.status);
        }

        /// <summary>
        /// Test 2: Test met null-waarden om te verifiëren dat deze naar standaardwaarden worden geconverteerd.
        /// Hiermee wordt de null-coalescing operator (??) getest.
        /// </summary>
        [Fact]
        public void VeilingProductInLaden_WithNullValues_ReturnsDefaultValues()
        {
            // ARRANGE - Maak een in-memory testcontext
            var context = TestHelpers.CreateInMemoryContext();
            
            // Maak een product met null-waarden zodat we de conversie naar default kunnen testen
            var product = new Product
            {
                IdProduct = 2,
                ProductNaam = null,              // null moet worden omgezet naar lege string
                ProductBeschrijving = null,      // null moet worden omgezet naar lege string
                Locatie = null,                  // null moet worden omgezet naar lege string
                Status = null                    // null moet worden omgezet naar 0
            };

            var controller = new ProductsController(context);

            // ACT - Roep de niet-publieke methode aan met reflection
            var method = typeof(ProductsController).GetMethod(
                "VeilingProductInLaden",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            var result = method?.Invoke(controller, new object[] { product }) as VeilingDTO;

            // ASSERT - Controleer dat null-waarden vervangen zijn door verwachte defaults
            Assert.NotNull(result);
            Assert.Equal(2, result.id);
            Assert.Equal(string.Empty, result.naam);           // Verwacht lege string, geen null
            Assert.Equal(string.Empty, result.beschrijving);   // Verwacht lege string, geen null
            Assert.Equal(string.Empty, result.locatie);        // Verwacht lege string, geen null
            Assert.Equal(0, result.status);                    // Verwacht 0, niet null
        }

        /// <summary>
        /// Test 3: Test met een mix van null en niet-null waarden
        /// </summary>
        [Fact]
        public void VeilingProductInLaden_WithMixedValues_ReturnsMixedResult()
        {
            // ARRANGE - Testcontext en product met gemengde waarden
            var context = TestHelpers.CreateInMemoryContext();
            
            var product = new Product
            {
                IdProduct = 3,
                ProductNaam = "Roses",
                ProductBeschrijving = null,     // Dit is null en moet worden omgezet
                Locatie = "Aalsmeer",
                Status = 2
            };

            var controller = new ProductsController(context);

            // ACT - Roep de private methode aan via reflection
            var method = typeof(ProductsController).GetMethod(
                "VeilingProductInLaden",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            var result = method?.Invoke(controller, new object[] { product }) as VeilingDTO;

            // ASSERT - Controleer dat niet-null waarden behouden blijven en nulls worden vervangen
            Assert.NotNull(result);
            Assert.Equal("Roses", result.naam);                // Heeft waarde
            Assert.Equal(string.Empty, result.beschrijving);   // Null omgezet naar lege string
            Assert.Equal("Aalsmeer", result.locatie);          // Heeft waarde
        }

        /// <summary>
        /// Test 4: Test met Status = 0 (standaardwaarde) om te verifiëren dat deze behouden blijft
        /// </summary>
        [Fact]
        public void VeilingProductInLaden_WithZeroStatus_ReturnsZero()
        {
            // ARRANGE - Testcontext en product met status 0
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

            // ACT - Roep de private methode aan
            var method = typeof(ProductsController).GetMethod(
                "VeilingProductInLaden",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            var result = method?.Invoke(controller, new object[] { product }) as VeilingDTO;

            // ASSERT - Controleer dat status 0 ongewijzigd blijft
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
        }
    }
}