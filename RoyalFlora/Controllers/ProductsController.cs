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
                startPrijs = product.StartPrijs,
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

            var koperGebruiker = await _context.Gebruikers.FindAsync(koperId);
            if (koperGebruiker == null) return Unauthorized();

            product.Koper = koperId;
            product.verkoopPrijs = dto.verkoopPrijs;

            var next = await _context.Products
            .Where(p => p.Status == 2 && (p.Locatie ?? "") == product.Locatie)
            .OrderBy(p => p.IdProduct)
            .FirstOrDefaultAsync();

            if (next != null)
            {
                next.Status = 3;
                _context.Entry(next).State = EntityState.Modified;
            }

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
                .Include(p => p.Fotos)
                .Include(p => p.KoperNavigation)
                .ToListAsync();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var gebruiker = await _context.Gebruikers
                .Include(g => g.BedrijfNavigation)
                .SingleOrDefaultAsync(g => g.IdGebruiker == userId);

            if (gebruiker == null) return Unauthorized();

            var bedrijf = gebruiker.BedrijfNavigation.BedrijfNaam;

            if (!string.IsNullOrWhiteSpace(location))
            {
                products = products
                    .Where(p => (p.Locatie ?? string.Empty)
                        .Equals(location, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var productDTOs = new List<ProductDTO>();
            var seenProductIds = new HashSet<int>();
            foreach (var product in products)
            {
                if (seenProductIds.Contains(product.IdProduct)) continue;

                var leverancierNaam = product.LeverancierNavigation?.BedrijfNaam ?? string.Empty;
                var datum = product.Datum?.ToString("yyyy-MM-dd") ?? string.Empty;
                var locatie = product.Locatie ?? string.Empty;
                var status = product.StatusNavigation?.Beschrijving ?? string.Empty;

                // If the product belongs to the same company as the current user, mark it as "eigen"
                if (leverancierNaam.Equals(bedrijf, StringComparison.OrdinalIgnoreCase))
                {
                    var eigendto = new ProductDTO
                    {
                        id = product.IdProduct,
                        naam = product.ProductNaam ?? string.Empty,
                        beschrijving = product.ProductBeschrijving ?? string.Empty,
                        merk = leverancierNaam,
                        verkoopPrijs = product.verkoopPrijs,
                        koper = (product.KoperNavigation?.VoorNaam ?? string.Empty) + " " + (product.KoperNavigation?.AchterNaam ?? string.Empty),
                        datum = datum,
                        locatie = locatie,
                        status = status,
                        aantal = product.Aantal,
                        fotoPath = product.Fotos.FirstOrDefault()?.FotoPath ?? string.Empty,
                        type = "eigen"
                    };
                    productDTOs.Add(eigendto);
                    seenProductIds.Add(product.IdProduct);
                    continue;
                }

                // If the product is already bought, mark it as "gekocht"
                if (status.Equals("gekocht", StringComparison.OrdinalIgnoreCase))
                {
                    var gekochtdto = new ProductDTO
                    {
                        id = product.IdProduct,
                        naam = product.ProductNaam ?? string.Empty,
                        beschrijving = product.ProductBeschrijving ?? string.Empty,
                        merk = leverancierNaam,
                        verkoopPrijs = product.verkoopPrijs,
                        datum = datum,
                        locatie = locatie,
                        status = status,
                        aantal = product.Aantal,
                        fotoPath = product.Fotos.FirstOrDefault()?.FotoPath ?? string.Empty,
                        type = "gekocht"
                    };
                    productDTOs.Add(gekochtdto);
                    seenProductIds.Add(product.IdProduct);
                    continue;
                }

                // Default case: regular product listing
                var dto = new ProductDTO
                {
                    id = product.IdProduct,
                    naam = product.ProductNaam ?? string.Empty,
                    beschrijving = product.ProductBeschrijving ?? string.Empty,
                    merk = leverancierNaam,
                    prijs = product.MinimumPrijs,
                    datum = datum,
                    locatie = locatie,
                    status = status,
                    aantal = product.Aantal,
                    fotoPath = product.Fotos.FirstOrDefault()?.FotoPath ?? string.Empty
                };
                productDTOs.Add(dto);
                seenProductIds.Add(product.IdProduct);
            }

            return productDTOs;
        }

        [HttpGet("Status1")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetStatus1Products()
        {
            var products = await _context.Products
                .Include(p => p.LeverancierNavigation)
                .Include(p => p.StatusNavigation)
                .Include(p => p.Fotos)
                .Include(p => p.KoperNavigation)
                .Where(p => p.Status == 1)
                .ToListAsync();

            var productDTOs = new List<ProductDTO>();
            foreach (var product in products)
            {
                var leverancierNaam = product.LeverancierNavigation?.BedrijfNaam ?? string.Empty;
                var datum = product.Datum?.ToString("yyyy-MM-dd") ?? string.Empty;
                var locatie = product.Locatie ?? string.Empty;
                var status = product.StatusNavigation?.Beschrijving ?? string.Empty;

                var dto = new ProductDTO
                {
                    id = product.IdProduct,
                    naam = product.ProductNaam ?? string.Empty,
                    beschrijving = product.ProductBeschrijving ?? string.Empty,
                    merk = leverancierNaam,
                    prijs = product.MinimumPrijs,
                    verkoopPrijs = product.verkoopPrijs,
                    koper = (product.KoperNavigation?.VoorNaam ?? string.Empty) + " " + (product.KoperNavigation?.AchterNaam ?? string.Empty),
                    datum = datum,
                    locatie = locatie,
                    status = status,
                    aantal = product.Aantal,
                    fotoPath = product.Fotos.FirstOrDefault()?.FotoPath ?? string.Empty
                };
                productDTOs.Add(dto);
            }

            return productDTOs;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return product;
        }

        //geen uses op het moment
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

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                
                foreach (var image in images)
                {
                    
                    string filePath = Path.Combine(uploadsFolder, image.FileName);
                    _context.Fotos.Add(new Foto
                    {
                        IdProduct = product.IdProduct,
                        FotoPath = image.FileName
                    });
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                }

                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetProduct), new { id = product.IdProduct }, new ResponseDTO { naam = product.ProductNaam ?? string.Empty, bericht = "Product succesvol geregistreerd!" });
            
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


        [HttpPost("productInplannen")]
        public async Task<IActionResult> productInplannen(
            [FromForm] int? id, 
            [FromForm] string? Datum,
            [FromForm] string? Tijd,
            [FromForm] string? StartPrijs)
        {
            var current = await _context.Products
                .Where(p => p.IdProduct == id && p.Status == 1)
                .FirstOrDefaultAsync();

            if (current == null) return NotFound("No valid product found");

            if (!decimal.TryParse(StartPrijs, NumberStyles.Any, CultureInfo.InvariantCulture, out var startPrijsValue))
                return BadRequest("Ongeldige startprijs");

            if (!DateTime.TryParseExact($"{Datum} {Tijd}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var geplandeMoment))
                return BadRequest("Ongeldige datum/tijd");

            current.StartPrijs = startPrijsValue;
            current.Datum = geplandeMoment;
            current.Status = 2;
            _context.Entry(current).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Product ingepland" });
        }

        private bool ProductExists(int id) => _context.Products.Any(e => e.IdProduct == id);
    }
}