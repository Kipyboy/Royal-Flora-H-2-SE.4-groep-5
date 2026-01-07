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
                    Status = 1
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

            current.Status = 4;
            _context.Entry(current).State = EntityState.Modified;

            var next = await _context.Products
                .Where(p => p.Status == 2 || p.Status == 5 && (p.Locatie ?? "") == locatie && p.Datum.Value.Date == DateTime.Today)
                .OrderBy(p => p.Datum)
                .FirstOrDefaultAsync();

            if (next == null)
            {
                await _context.SaveChangesAsync();
                return NotFound("No next product available");
            }
            if (next.Status == 5) {
                return NotFound("Next product was paused");
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

        // Get all products with status 4, returning naam and verkoopprijs
        // Supports optional case-insensitive filter by product name using raw SQL
        [HttpGet("Status4Products")]
        public async Task<ActionResult<IEnumerable<Status4ProductDTO>>> GetStatus4Products([FromQuery] string? naamFilter = null)
        {
            try
            {
                IEnumerable<Status4ProductDTO> products;

                if (string.IsNullOrEmpty(naamFilter))
                {
                    // No filter: select all products with status 4
                    string sqlWithIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs FROM Products WITH (INDEX(IX_Products_Status_ProductNaam)) WHERE Status = 4";
                    string sqlNoIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs FROM Products WHERE Status = 4";
                    products = await SqlQueryWithIndexFallback<Status4ProductDTO>(sqlWithIndex, sqlNoIndex);
                }
                else
                {
                    // With filter: case-insensitive LIKE search using SQL parameters
                    string sqlWithIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs FROM Products WITH (INDEX(IX_Products_Status_ProductNaam)) WHERE Status = 4 AND ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS LIKE '%' + @naamFilter + '%'";
                    string sqlNoIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs FROM Products WHERE Status = 4 AND ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS LIKE '%' + @naamFilter + '%'";
                    var param = new Microsoft.Data.SqlClient.SqlParameter("@naamFilter", naamFilter);
                    products = await SqlQueryWithIndexFallback<Status4ProductDTO>(sqlWithIndex, sqlNoIndex, param);
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error retrieving products", details = ex.Message });
            }
        }

        [HttpGet("VeilingSoldMatches")]
        public async Task<ActionResult<VeilingSoldMatchesDTO>> GetVeilingSoldMatches([FromQuery] string locatie)
        {
            if (string.IsNullOrWhiteSpace(locatie)) return BadRequest("Missing locatie");

            var active = await _context.Products
                .Where(p => p.Status == 3 && (p.Locatie ?? "").ToLower() == locatie.ToLower())
                .FirstOrDefaultAsync();

            if (active == null) return NotFound("No active veiling for locatie");

            var naam = active.ProductNaam ?? string.Empty;

            string sqlWithIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs, Aantal FROM Products WITH (INDEX(IX_Products_Status_ProductNaam)) WHERE Status = 4 AND ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam";
            string sqlNoIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs, Aantal FROM Products WHERE Status = 4 AND ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam";
            var param = new Microsoft.Data.SqlClient.SqlParameter("@naam", naam);

            var sold = await SqlQueryWithIndexFallback<SoldItemDTO>(sqlWithIndex, sqlNoIndex, param);

            var result = new VeilingSoldMatchesDTO
            {
                ActiveProductId = active.IdProduct,
                ActiveProductNaam = naam,
                SoldProducts = sold
            };

            return Ok(result);
        }

        private bool ProductExists(int id) => _context.Products.Any(e => e.IdProduct == id);
        
        private async Task<List<T>> SqlQueryWithIndexFallback<T>(string sqlWithIndex, string sqlWithoutIndex, params object[] parameters)
        {
            try
            {
                return await _context.Database.SqlQueryRaw<T>(sqlWithIndex, parameters).ToListAsync();
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 308)
            {
                // Index specified in hint does not exist; retry without index hint
                return await _context.Database.SqlQueryRaw<T>(sqlWithoutIndex, parameters).ToListAsync();
            }
        }

        [HttpGet("CheckIndex")]
        public async Task<ActionResult<bool>> CheckIndex()
        {
            try
            {
                var rows = await _context.Database.SqlQueryRaw<int>("SELECT 1 FROM sys.indexes WHERE name='IX_Products_Status_ProductNaam' AND object_id = OBJECT_ID('Products')").ToListAsync();
                return Ok(rows.Any());
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error checking index", details = ex.Message });
            }
        }

        [HttpGet("ListProductIndexes")]
        public async Task<ActionResult<IEnumerable<string>>> ListProductIndexes()
        {
            try
            {
                var rows = await _context.Database.SqlQueryRaw<string>("SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID('Products')").ToListAsync();
                return Ok(rows);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error listing indexes", details = ex.Message });
            }
        }

        [HttpPost("EnsureIndex")]
        public async Task<IActionResult> EnsureIndex()
        {
            try
            {
                var sql = @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Status_ProductNaam' AND object_id = OBJECT_ID('Products'))
                            BEGIN
                                CREATE NONCLUSTERED INDEX IX_Products_Status_ProductNaam ON Products (Status, ProductNaam)
                            END";
                await _context.Database.ExecuteSqlRawAsync(sql);
                return Ok(new { message = "Index ensured" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to create index", details = ex.Message });
            }
        }
        [Authorize(Roles = "Veilingmeester")]
        [HttpPost("StartAuctions")]
        public async Task<ActionResult> StartAuctions ()
        {
            var today = DateTime.Today;
            var now = DateTime.Now;

            var scheduledToday = await _context.Products
                .Where(p => p.Status == 2 && p.Datum.HasValue && p.Datum.Value.Date == today)
                .ToListAsync();

            if (!scheduledToday.Any())
                return NotFound("No auctions scheduled for today");

            var activated = new List<object>();

            var byLocation = scheduledToday.GroupBy(p => (p.Locatie ?? string.Empty));

            foreach (var group in byLocation)
            {
                var due = group.Where(p => p.Datum.HasValue && p.Datum.Value <= now)
                               .OrderBy(p => p.Datum)
                               .FirstOrDefault();

                var toActivate = due ?? group.OrderBy(p => p.Datum).FirstOrDefault();

                if (toActivate != null)
                {
                    toActivate.Status = 3;
                    _context.Entry(toActivate).State = EntityState.Modified;
                    activated.Add(new { locatie = group.Key, id = toActivate.IdProduct, startTime = toActivate.Datum });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(activated);
        }
        
        [Authorize(Roles = "Veilingmeester")]
        [HttpPost("PauseAuctions")]
        public async Task<ActionResult> PauseAuctions ()
        {
            var now = DateTime.Now;
            var today = DateTime.Today;

            var toPause = await _context.Products
                .Where(p => p.Status == 2 && p.Datum.HasValue && p.Datum.Value.Date == today)
                .ToListAsync();

            var paused = new List<object>();

            var byLocation = toPause.GroupBy(p => (p.Locatie ?? string.Empty));

            foreach (var group in byLocation)
            {
                var due = group.Where(p => p.Datum.HasValue && p.Datum.Value <= now)
                               .OrderBy(p => p.Datum)
                               .FirstOrDefault();

                var toBePaused = due ?? group.OrderBy(p => p.Datum).FirstOrDefault();

                if (toBePaused != null)
                {
                    toBePaused.Status = 5;
                    _context.Entry(toBePaused).State = EntityState.Modified;
                    paused.Add(new { locatie = group.Key, id = toBePaused.IdProduct, startTime = toBePaused.Datum });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(paused);
        }

        [HttpGet("HasPausedAuctions")]
        public async Task<ActionResult<bool>> HasPausedAuctions()
        {
            try
            {
                var any = await _context.Products.AnyAsync(p => p.Status == 5);
                return Ok(any);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Error checking paused auctions", details = ex.Message });
            }
        }

        [Authorize(Roles = "Veilingmeester")]
        [HttpPost("ResumeAuctions")]
        public async Task<IActionResult> ResumeAuctions()
        {
            var paused = await _context.Products
                .Where(p => p.Status == 5)
                .ToListAsync();

            if (!paused.Any()) return NotFound(new { message = "No paused auctions found" });

            foreach (var p in paused)
            {
                
                p.Status = 3;
                _context.Entry(p).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return Ok(new { resumedCount = paused.Count, ids = paused.Select(p => p.IdProduct) });
        }
    }
}