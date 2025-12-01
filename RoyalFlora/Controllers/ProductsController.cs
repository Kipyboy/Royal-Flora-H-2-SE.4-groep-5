using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalFlora.Migrations;

namespace RoyalFlora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ProductsController(MyDbContext context)
        {
            _context = context;
        }

        private VeilingDTO VeilingProductInLaden(Product product)
        {
            return new VeilingDTO
            {
                id = product.IdProduct,
                naam = product.ProductNaam ?? string.Empty,
                beschrijving = product.ProductBeschrijving ?? string.Empty,
                locatie = product.Locatie ?? string.Empty,
                status = product.Status ?? 0,
            };
        }

        [HttpGet("Veiling")]
        public async Task<ActionResult<IEnumerable<VeilingDTO>>> GetVeilingProducts()
        {
            var products = await _context.Products.ToListAsync();
            return products.Select(VeilingProductInLaden).ToList();
        }

        private ClockDTO PrijsVoorKlok(Product product)
        {
            return new ClockDTO
            {
                minimumPrijs = product.MinimumPrijs,
                locatie = product.Locatie ?? string.Empty,
                status = product.Status
            };
        }

        [HttpGet("Klok")]
        public async Task<ActionResult<ClockDTO>> GetKlokPrijs([FromQuery] string locatie)
        {
            var product = await _context.Products
                .Where(p => p.Status == 3 && (p.Locatie ?? "").ToLower() == locatie.ToLower())
                .FirstOrDefaultAsync();

            if (product == null) return NotFound();
            return PrijsVoorKlok(product);
        }

        [Authorize]
        [HttpPatch("{id:int}/koop")]
        public async Task<IActionResult> KoopProduct(int id, [FromBody] KoopDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId == null) return Unauthorized();

            product.Status = 4;
            if (!int.TryParse(userId, out int koperId)) return Unauthorized();
            product.Koper = koperId;
            product.verkoopPrijs = dto.verkoopPrijs;

            _context.Entry(product).State = EntityState.Modified;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts([FromQuery] string? location)
        {
            var products = await _context.Products
                .Include(p => p.LeverancierNavigation)
                .Include(p => p.StatusNavigation)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(location))
            {
                products = products
                    .Where(p => (p.Locatie ?? string.Empty)
                        .Equals(location, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var productDTOs = products.Select(product => new ProductDTO
            {
                id = product.IdProduct,
                naam = product.ProductNaam ?? string.Empty,
                merk = product.LeverancierNavigation?.BedrijfNaam ?? string.Empty,
                prijs = product.MinimumPrijs,
                datum = product.Datum?.ToString("yyyy-MM-dd") ?? string.Empty,
                locatie = product.Locatie ?? string.Empty,
                status = product.StatusNavigation?.Beschrijving ?? string.Empty,
                aantal = product.Aantal
            }).ToList();

            return productDTOs;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return product;
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.IdProduct) return BadRequest();

            _context.Entry(product).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ProductDTO>> PostProduct([FromForm] string? ProductNaam, 
            [FromForm] string? ProductBeschrijving, 
            [FromForm] string? MinimumPrijs, 
            [FromForm] string? Locatie,
            [FromForm] string? Datum,
            [FromForm] string? Aantal,
            [FromForm] string? Leverancier,
            [FromForm] List<IFormFile> images)
        {
            try
            {
                Console.WriteLine($"DEBUG: Received ProductNaam={ProductNaam}, MinimumPrijs={MinimumPrijs}, Locatie={Locatie}, Datum={Datum}, Aantal={Aantal}, Leverancier={Leverancier}");

                // Parse MinimumPrijs with invariant culture to handle decimal correctly
                decimal minimumPrijsValue = 0;
                if (!string.IsNullOrWhiteSpace(MinimumPrijs))
                {
                    if (decimal.TryParse(MinimumPrijs, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed))
                    {
                        minimumPrijsValue = parsed;
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: Failed to parse MinimumPrijs: {MinimumPrijs}");
                        return BadRequest(new { message = "Registratie mislukt", details = $"Invalid price format: {MinimumPrijs}" });
                    }
                }

                // Parse Aantal
                int? aantalValue = null;
                if (!string.IsNullOrWhiteSpace(Aantal) && int.TryParse(Aantal, out int aantalParsed))
                {
                    aantalValue = aantalParsed;
                }

                // Parse Leverancier (KVK)
                int? leverancierValue = null;
                if (!string.IsNullOrWhiteSpace(Leverancier) && int.TryParse(Leverancier, out int leverancierParsed))
                {
                    leverancierValue = leverancierParsed;
                    Console.WriteLine($"DEBUG: Parsed Leverancier={leverancierValue}");
                    
                    // Check if this KVK exists in Bedrijf table
                    var bedrijfExists = _context.Bedrijven.Any(b => b.KVK == leverancierValue);
                    Console.WriteLine($"DEBUG: KVK {leverancierValue} exists in Bedrijf table: {bedrijfExists}");
                }
                else
                {
                    Console.WriteLine($"ERROR: Failed to parse Leverancier: {Leverancier}");
                }
                DateTime? datumValue = null;
                if (!string.IsNullOrWhiteSpace(Datum))
                {
                    if (DateTime.TryParse(Datum, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datumParsed))
                    {
                        datumValue = datumParsed;
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: Failed to parse Datum: {Datum}");
                        return BadRequest(new { message = "Registratie mislukt", details = $"Invalid date format: {Datum}" });
                    }
                }

                var product = new Product
                {
                    ProductNaam = ProductNaam,
                    ProductBeschrijving = ProductBeschrijving,
                    MinimumPrijs = minimumPrijsValue,
                    Locatie = Locatie,
                    Datum = datumValue,
                    Aantal = aantalValue,
                    Leverancier = leverancierValue,
                    Status = null  // Status will be set later or is optional
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (images != null && images.Count > 0)
                {
                    _context.Fotos.AddRange(images.Select(img => new Foto
                    {
                        IdProduct = product.IdProduct,
                        FotoPath = img.FileName
                    }));
                    await _context.SaveChangesAsync();
                }

                var productDTO = new ProductDTO
                {
                    id = product.IdProduct,
                    naam = product.ProductNaam ?? string.Empty,
                    merk = string.Empty,
                    prijs = product.MinimumPrijs,
                    datum = product.Datum?.ToString("yyyy-MM-dd") ?? string.Empty,
                    locatie = product.Locatie ?? string.Empty,
                    status = string.Empty,
                    aantal = product.Aantal
                };

                return Ok(productDTO);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"STACKTRACE: {ex.StackTrace}");
                return BadRequest(new { message = "Registratie mislukt", details = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("Advance")]
        public async Task<IActionResult> Advance([FromQuery] string locatie)
        {
            var current = await _context.Products
                .Where(p => p.Status == 3 && (p.Locatie ?? "") == locatie)
                .FirstOrDefaultAsync();

            if (current == null) return NotFound("No active product found");

            current.Status = 5;
            _context.Entry(current).State = EntityState.Modified;

            var next = await _context.Products
                .Where(p => p.Status == 2 && (p.Locatie ?? "") == locatie)
                .OrderBy(p => p.IdProduct)
                .FirstOrDefaultAsync();

            if (next == null)
            {
                await _context.SaveChangesAsync();
                return NotFound("No next product available");
            }

            next.Status = 3;
            _context.Entry(next).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return Ok(new { nextId = next.IdProduct });
        }

        private bool ProductExists(int id) => _context.Products.Any(e => e.IdProduct == id);
    }
}
