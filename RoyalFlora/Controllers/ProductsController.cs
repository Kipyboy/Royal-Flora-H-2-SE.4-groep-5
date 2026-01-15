using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        // Constructor: krijg een instantie van de databasecontext via dependency injection
        public ProductsController(MyDbContext context)
        {
            _context = context;
        }

        // Mapper: zet een Product om naar een VeilingDTO die door de veiling-endpoint wordt gebruikt.
        private VeilingDTO VeilingProductInLaden(Product product)
        {
            // Zorg voor veilige default-waardes bij null-velden
            return new VeilingDTO
            {
                id = product.IdProduct,
                naam = product.ProductNaam ?? string.Empty,
                beschrijving = product.ProductBeschrijving ?? string.Empty,
                locatie = product.Locatie ?? string.Empty,
                status = product.Status ?? 0,
            };
        }

        // Endpoint: retourneer alle producten in het formaat dat de veiling nodig heeft
        [HttpGet("Veiling")]
        public async Task<ActionResult<IEnumerable<VeilingDTO>>> GetVeilingProducts()
        {
            var products = await _context.Products.ToListAsync();
            return products.Select(VeilingProductInLaden).ToList();
        }

        // Mapper: zet prijsgerelateerde velden om naar de ClockDTO (gebruik voor live klok/veiling)
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

        // Endpoint: haal de prijsinformatie voor de huidige actieve klok (status 3) op basis van locatie
        [HttpGet("Klok")]
        public async Task<ActionResult<ClockDTO>> GetKlokPrijs([FromQuery] string locatie)
        {
            var product = await _context.Products
                .Where(p => p.Status == 3 && (p.Locatie ?? "").ToLower() == locatie.ToLower())
                .FirstOrDefaultAsync();

            if (product == null) return NotFound();
            return PrijsVoorKlok(product);
        }

        // Endpoint: Authenticated gebruiker koopt een product.
        // - Update status naar verkocht (4)
        // - Sla koper en verkoopprijs op
        // - Activeer het volgende product (status 3) in dezelfde locatie indien aanwezig
        [Authorize]
        [HttpPatch("{id:int}/koop")]
        public async Task<IActionResult> KoopProduct(int id, [FromBody] KoopDto dto)
        {
            // Zoek het product op
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Haal de gebruiker-id uit de JWT-claims (NameIdentifier of subject)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId == null) return Unauthorized();

            product.Status = 4;
            // Converteer userId naar int (IdGebruiker)
            if (!int.TryParse(userId, out int koperId)) return Unauthorized();

            // Controleer dat de koper in de database bestaat
            var koperGebruiker = await _context.Gebruikers.FindAsync(koperId);
            if (koperGebruiker == null) return Unauthorized();

            product.Koper = koperId;
            product.verkoopPrijs = dto.verkoopPrijs;

            // Zoek het volgende product in de planning (status 2) voor dezelfde locatie en zet deze actief
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

        // Endpoint: Haal producten op (optioneel gefilterd op `location`).
        // Deze endpoint gebruikt de huidige gebruiker om producten als 'eigen' of 'gekocht' te markeren.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts([FromQuery] string? location)
        {
            var products = await _context.Products
                .Include(p => p.LeverancierNavigation)
                .Include(p => p.StatusNavigation)
                .Include(p => p.Foto)
                .Include(p => p.KoperNavigation)
                .ToListAsync();

            // Haal huidige gebruiker uit de claims; vereist voor bepalen van 'eigen' producten
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            // Haal gebruiker inclusief bedrijf zodat we kunnen vergelijken of product van hetzelfde bedrijf is
            var gebruiker = await _context.Gebruikers
                .Include(g => g.BedrijfNavigation)
                .SingleOrDefaultAsync(g => g.IdGebruiker == userId);

            if (gebruiker == null) return Unauthorized();

            // BedrijfNavigation may be null in some DB states; guard against it
            var bedrijf = gebruiker.BedrijfNavigation?.BedrijfNaam ?? string.Empty;

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

                // Als product van hetzelfde bedrijf is als de gebruiker: markeer als 'eigen'
                if (leverancierNaam.Equals(bedrijf, StringComparison.OrdinalIgnoreCase))
                {
                    var eigendto = new ProductDTO
                    {
                        id = product.IdProduct,
                        naam = product.ProductNaam ?? string.Empty,
                        beschrijving = product.ProductBeschrijving ?? string.Empty,
                        merk = leverancierNaam,
                        verkoopPrijs = product.verkoopPrijs ?? 0,
                        koper = (product.KoperNavigation?.VoorNaam ?? string.Empty) + " " + (product.KoperNavigation?.AchterNaam ?? string.Empty),
                        datum = datum,
                        locatie = locatie,
                        status = status,
                        aantal = product.Aantal,
                        fotoPath = product.Foto?.FotoPath ?? string.Empty,
                        type = "eigen"
                    };
                    productDTOs.Add(eigendto);
                    seenProductIds.Add(product.IdProduct);
                    continue;
                }

                // Als product status 'gekocht' heeft: markeer als 'gekocht'
                if (status.Equals("gekocht", StringComparison.OrdinalIgnoreCase))
                {
                    var gekochtdto = new ProductDTO
                    {
                        id = product.IdProduct,
                        naam = product.ProductNaam ?? string.Empty,
                        beschrijving = product.ProductBeschrijving ?? string.Empty,
                        merk = leverancierNaam,
                        verkoopPrijs = product.verkoopPrijs ?? 0,
                        datum = datum,
                        locatie = locatie,
                        status = status,
                        aantal = product.Aantal,
                        fotoPath = product.Foto?.FotoPath ?? string.Empty,
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
                    fotoPath = product.Foto?.FotoPath ?? string.Empty
                };
                productDTOs.Add(dto);
                seenProductIds.Add(product.IdProduct);
            }

            return productDTOs;
        }

        // Endpoint: Haal producten met status 1 op (bijvoorbeeld nieuw/ingediend)
        [HttpGet("Status1")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetStatus1Products()
        {
            var products = await _context.Products
                .Include(p => p.LeverancierNavigation)
                .Include(p => p.StatusNavigation)
                .Include(p => p.Foto)
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
                    verkoopPrijs = product.verkoopPrijs ?? 0,
                    koper = (product.KoperNavigation?.VoorNaam ?? string.Empty) + " " + (product.KoperNavigation?.AchterNaam ?? string.Empty),
                    datum = datum,
                    locatie = locatie,
                    status = status,
                    aantal = product.Aantal,
                    fotoPath = product.Foto?.FotoPath ?? string.Empty
                };
                productDTOs.Add(dto);
            }

            return productDTOs;
        }


        // Endpoint: registreer een nieuw product via multipart/form-data.
        // Verwerkt velden als MinimumPrijs, Datum en optioneel 1 afbeelding (images).
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

                // Parse MinimumPrijs: gebruik InvariantCulture zodat decimal separators (punt/komma) uniform worden verwerkt
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

                // Parse Aantal: indien opgegeven, zet string om naar integer
                int? aantalValue = null;
                if (!string.IsNullOrWhiteSpace(Aantal) && int.TryParse(Aantal, out int aantalParsed))
                {
                    aantalValue = aantalParsed;
                }

                // Parse Leverancier (KVK): verwacht een numerieke KVK; log of er een overeenkomend bedrijf bestaat
                int? leverancierValue = null;
                if (!string.IsNullOrWhiteSpace(Leverancier) && int.TryParse(Leverancier, out int leverancierParsed))
                {
                    leverancierValue = leverancierParsed;
                    Console.WriteLine($"DEBUG: Parsed Leverancier={leverancierValue}");
                    
                    // Controleer of bedrijf met deze KVK bestaat (alleen voor debug/logging)
                    var bedrijfExists = _context.Bedrijven.Any(b => b.KVK == leverancierValue);
                    Console.WriteLine($"DEBUG: KVK {leverancierValue} exists in Bedrijf table: {bedrijfExists}");
                }
                else
                {
                    Console.WriteLine($"ERROR: Failed to parse Leverancier: {Leverancier}");
                }
                // Parse Datum: zet string om naar DateTime met invariant culture indien opgegeven
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

                // Maak nieuw Product-object en zet initiële velden; Status 1 = nieuw/ingediend
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
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine("ERROR saving product: " + dbEx.GetBaseException().Message);
                    return BadRequest(new { message = "Registratie mislukt", details = dbEx.GetBaseException().Message });
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Afbeelding verwerken: DB staat nu slechts één Foto per product toe, dus update of voeg toe
                if (images != null && images.Count > 0)
                {
                    var image = images.First();
                    // Let op: FileName komt van de client. In productie graag een veilige naam genereren (bijv. GUID) en controleren op path traversal
                    string filePath = Path.Combine(uploadsFolder, image.FileName);

                    var existingFoto = await _context.Fotos.FirstOrDefaultAsync(f => f.IdProduct == product.IdProduct);
                    if (existingFoto != null)
                    {
                        existingFoto.FotoPath = image.FileName;
                        _context.Entry(existingFoto).State = EntityState.Modified;
                    }
                    else
                    {
                        _context.Fotos.Add(new Foto
                        {
                            IdProduct = product.IdProduct,
                            FotoPath = image.FileName
                        });
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine("ERROR saving foto: " + dbEx.GetBaseException().Message);
                    return BadRequest(new { message = "Registratie mislukt", details = dbEx.GetBaseException().Message });
                }
                return CreatedAtAction(nameof(GetProducts), new { }, new ResponseDTO { naam = product.ProductNaam ?? string.Empty, bericht = "Product succesvol geregistreerd!" });
            
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
            var foto = await _context.Fotos.Where(f => f.IdProduct == id).FirstOrDefaultAsync();
            if (product == null) return NotFound();


            if (foto != null) {
            _context.Fotos.Remove(foto);
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Endpoint: ga naar het volgende product in de veiling voor een locatie
        // Werkwijze:
        //  - Vind huidig actief product (status 3) voor de locatie
        //  - Zoek volgend gepland product (status 2 of 5) met datum = vandaag; activeer het als beschikbaar
        //  - Als er geen volgend product is, herstel huidige naar status 1 (tenzij verkocht)
        //  - Reset de klok voor de locatie via ClockTimerService
        [HttpPost("Advance")]
        public async Task<IActionResult> Advance([FromQuery] string locatie)
        {
            var current = await _context.Products
                .Where(p => p.Status == 3 && (p.Locatie ?? "") == locatie)
                .FirstOrDefaultAsync();

            if (current == null) return NotFound("No active product found");

            
            _context.Entry(current).State = EntityState.Modified;

            var next = await _context.Products
                .Where(p => (p.Status == 2 || p.Status == 5) && (p.Locatie ?? "") == locatie && p.Datum.HasValue && p.Datum.Value.Date == DateTime.Today)
                .OrderBy(p => p.IdProduct)
                .FirstOrDefaultAsync();

            if (next == null)
            {
                // No next product available. Ensure we clear the "active" status
                // for the current product so it doesn't keep looping.
                var dbStatusCurrent = await _context.Products
                    .Where(p => p.IdProduct == current.IdProduct)
                    .Select(p => p.Status)
                    .FirstOrDefaultAsync();

                if (dbStatusCurrent != 4)
                {
                    // If current was not sold, revert it to status 1 (not active)
                    current.Status = 1;
                    _context.Entry(current).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                return NotFound("No next product available");
            }
            if (next.Status == 5) {
                await _context.SaveChangesAsync();
                return NotFound("Next product was paused");
            }

            next.Status = 3;
            _context.Entry(next).State = EntityState.Modified;

            ClockTimerService.ResetClockForLocation(locatie);

            var dbStatus = await _context.Products
            .Where(p => p.IdProduct == current.IdProduct)
            .Select(p => p.Status)
            .FirstOrDefaultAsync();

            if (dbStatus != 4)
            {
                current.Status = 1;
                _context.Entry(current).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return Ok(new { nextId = next.IdProduct });
        }


        // Endpoint: plan een product in voor veiling (zet startprijs en geplande datum, status -> 2)
        [HttpPost("productInplannen")]
        public async Task<IActionResult> productInplannen(
            [FromForm] int? id, 
            [FromForm] string? Datum,
            [FromForm] string? StartPrijs)
        {
            // Zoek product dat nog niet gepland is (status 1)
            var current = await _context.Products
                .Where(p => p.IdProduct == id && p.Status == 1)
                .FirstOrDefaultAsync();

            if (current == null) return NotFound("No valid product found");

            // Parse en valideer startprijs en datum; beide moeten in het juiste formaat zijn
            if (!decimal.TryParse(StartPrijs, NumberStyles.Any, CultureInfo.InvariantCulture, out var startPrijsValue))
                return BadRequest("Ongeldige startprijs");

            if (!DateTime.TryParseExact($"{Datum}", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var geplandeMoment))
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
                    // Geen filter: haal alle verkochte producten (status 4) met verkoopprijs op
                    string sqlWithIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs FROM Products WITH (INDEX(IX_Products_Status_ProductNaam)) WHERE Status = 4 AND verkoopPrijs IS NOT NULL";
                    string sqlNoIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs FROM Products WHERE Status = 4 AND verkoopPrijs IS NOT NULL";
                    products = await SqlQueryWithIndexFallback<Status4ProductDTO>(sqlWithIndex, sqlNoIndex);
                }
                else
                {
                    // Met filter: case-insensitieve LIKE-search, parameterized om SQL-injectie te voorkomen
                    string sqlWithIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs FROM Products WITH (INDEX(IX_Products_Status_ProductNaam)) WHERE Status = 4 AND verkoopPrijs IS NOT NULL AND ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS LIKE '%' + @naamFilter + '%'";
                    string sqlNoIndex = "SELECT IdProduct, ProductNaam, verkoopPrijs FROM Products WHERE Status = 4 AND verkoopPrijs IS NOT NULL AND ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS LIKE '%' + @naamFilter + '%'";
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
        
        //op het moment geen uses
        [HttpGet("VeilingSoldMatches")]
        public async Task<ActionResult<VeilingSoldMatchesDTO>> GetVeilingSoldMatches([FromQuery] string locatie)
        {
            if (string.IsNullOrWhiteSpace(locatie)) return BadRequest("Missing locatie");

            var active = await _context.Products
                .Where(p => p.Status == 3 && (p.Locatie ?? "").ToLower() == locatie.ToLower())
                .FirstOrDefaultAsync();

            if (active == null) return NotFound("No active veiling for locatie");

            string naam = active.ProductNaam ?? string.Empty;
            var leverancierId = active.Leverancier;

                        // Raw SQL: haal recente verkochte items die exact passen op naam en (optioneel) leverancier
                        // Met index-hint voor performance; we vervangen later de naam-vergelijking door LOWER(...)=@lowerName voor case-insensitive matching
                        var sqlWithIndex = @"SELECT TOP (10) p.IdProduct,
             p.ProductNaam,
             p.verkoopPrijs AS VerkoopPrijs,
             p.Aantal,
             FORMAT(p.Datum, 'dd-MM-yyyy') AS SoldDate,
             b.BedrijfNaam AS AanvoerderNaam
FROM Products p WITH (INDEX(IX_Products_ProductNaam_Leverancier_IdProduct))
LEFT JOIN Bedrijf b ON p.Leverancier = b.KVK
WHERE p.Status = 4
    AND p.ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam
    AND ((@leverancier IS NULL AND p.Leverancier IS NULL) OR (p.Leverancier = @leverancier))
    AND p.verkoopPrijs IS NOT NULL
ORDER BY p.Datum DESC, p.IdProduct DESC";

                        var sqlNoIndex = @"SELECT TOP (10) p.IdProduct,
             p.ProductNaam,
             p.verkoopPrijs AS VerkoopPrijs,
             p.Aantal,
             FORMAT(p.Datum, 'dd-MM-yyyy') AS SoldDate,
             b.BedrijfNaam AS AanvoerderNaam
FROM Products p
LEFT JOIN Bedrijf b ON p.Leverancier = b.KVK
WHERE p.Status = 4
    AND p.ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam
    AND ((@leverancier IS NULL AND p.Leverancier IS NULL) OR (p.Leverancier = @leverancier))
    AND p.verkoopPrijs IS NOT NULL
ORDER BY p.Datum DESC, p.IdProduct DESC";

                        // Gebruik lower-case vergelijking om hoofdletter- en whitespace-verschillen te negeren
                        var lowerName = (naam ?? string.Empty).Trim().ToLower();
                        var pLower = new Microsoft.Data.SqlClient.SqlParameter("@lowerName", lowerName);
                        var pLever = new Microsoft.Data.SqlClient.SqlParameter("@leverancier", leverancierId ?? (object)DBNull.Value);

                        // Replace name parameter usage in SQL to use LOWER(p.ProductNaam) = @lowerName
                        sqlWithIndex = sqlWithIndex.Replace("p.ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam", "LOWER(p.ProductNaam) = @lowerName");
                        sqlNoIndex = sqlNoIndex.Replace("p.ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam", "LOWER(p.ProductNaam) = @lowerName");

                        var sold = await SqlQueryWithIndexFallback<SoldItemDTO>(sqlWithIndex, sqlNoIndex, pLower, pLever);

                        // Bereken gemiddelde verkoopprijs via SQL. Als de index-hint faalt, wordt er teruggevallen naar dezelfde query zonder hint
                        var avgSqlWithIndex = @"SELECT AVG(CONVERT(decimal(18,2), p.verkoopPrijs))
FROM Products p WITH (INDEX(IX_Products_ProductNaam_Leverancier_IdProduct))
WHERE p.Status = 4
    AND p.ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam
    AND ((@leverancier IS NULL AND p.Leverancier IS NULL) OR (p.Leverancier = @leverancier))
    AND p.verkoopPrijs IS NOT NULL";
                        var avgSqlNoIndex = @"SELECT AVG(CONVERT(decimal(18,2), p.verkoopPrijs))
FROM Products p
WHERE p.Status = 4
    AND p.ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam
    AND ((@leverancier IS NULL AND p.Leverancier IS NULL) OR (p.Leverancier = @leverancier))
    AND p.verkoopPrijs IS NOT NULL";

                        decimal? avg = null;
                        try
                        {
                                // adjust avg SQL params to use lowerName as well
                                avgSqlWithIndex = avgSqlWithIndex.Replace("p.ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam", "LOWER(p.ProductNaam) = @lowerName");
                                avgSqlNoIndex = avgSqlNoIndex.Replace("p.ProductNaam COLLATE SQL_Latin1_General_CP1_CI_AS = @naam", "LOWER(p.ProductNaam) = @lowerName");
                                var avgList = await SqlQueryWithIndexFallback<decimal?>(avgSqlWithIndex, avgSqlNoIndex, pLower, pLever);
                                avg = avgList.FirstOrDefault();
                        }
                        catch
                        {
                                var prices = sold.Where(s => s.VerkoopPrijs.HasValue).Select(s => s.VerkoopPrijs!.Value).ToList();
                                if (prices.Any()) avg = Math.Round(prices.Average(), 2);
                        }

            var result = new VeilingSoldMatchesDTO
            {
                ActiveProductId = active.IdProduct,
                ActiveProductNaam = naam ?? string.Empty,
                SoldProducts = sold,
                AverageVerkoopPrijs = avg
            };

            return Ok(result);
        }

        // Prijsgeschiedenis endpoint: filter op productnaam (exact), retourneer de 10 meest recente verkochte items en gemiddelden
        [HttpGet("PriceHistory")]
        public async Task<ActionResult<PriceHistoryResultDTO>> GetPriceHistory([FromQuery] string naam)
        {
            if (string.IsNullOrWhiteSpace(naam)) return BadRequest("Missing naam parameter");

            var lowerName = naam.ToLower();
            // Raw SQL voor prijsgeschiedenis: top 10 meest recente verkochte items (op Datum desc), met geformatteerde datum en leverancier
            var phSqlWithIndex = @"SELECT TOP (10) p.IdProduct,
       p.ProductNaam,
       p.verkoopPrijs AS VerkoopPrijs,
       FORMAT(p.Datum, 'dd-MM-yyyy') AS SoldDate,
       b.BedrijfNaam AS AanvoerderNaam
FROM Products p WITH (INDEX(IX_Products_ProductNaam_Leverancier_IdProduct))
LEFT JOIN Bedrijf b ON p.Leverancier = b.KVK
WHERE p.Status = 4 AND p.ProductNaam IS NOT NULL AND LOWER(p.ProductNaam) = @lowerName AND p.verkoopPrijs IS NOT NULL
ORDER BY COALESCE(p.Datum, '1900-01-01') DESC, p.IdProduct DESC";

            var phSqlNoIndex = @"SELECT TOP (10) p.IdProduct,
       p.ProductNaam,
       p.verkoopPrijs AS VerkoopPrijs,
       FORMAT(p.Datum, 'dd-MM-yyyy') AS SoldDate,
       b.BedrijfNaam AS AanvoerderNaam
FROM Products p
LEFT JOIN Bedrijf b ON p.Leverancier = b.KVK
WHERE p.Status = 4 AND p.ProductNaam IS NOT NULL AND LOWER(p.ProductNaam) = @lowerName AND p.verkoopPrijs IS NOT NULL
ORDER BY COALESCE(p.Datum, '1900-01-01') DESC, p.IdProduct DESC";

            var pLower = new Microsoft.Data.SqlClient.SqlParameter("@lowerName", lowerName);
            var items = await SqlQueryWithIndexFallback<PriceHistoryItemDTO>(phSqlWithIndex, phSqlNoIndex, pLower);

            // Average
            var avgPhWithIndex = @"SELECT AVG(CONVERT(decimal(18,2), p.verkoopPrijs))
FROM Products p WITH (INDEX(IX_Products_ProductNaam_Leverancier_IdProduct))
WHERE p.Status = 4 AND p.ProductNaam IS NOT NULL AND LOWER(p.ProductNaam) = @lowerName AND p.verkoopPrijs IS NOT NULL";
            var avgPhNoIndex = @"SELECT AVG(CONVERT(decimal(18,2), p.verkoopPrijs))
FROM Products p
WHERE p.Status = 4 AND p.ProductNaam IS NOT NULL AND LOWER(p.ProductNaam) = @lowerName AND p.verkoopPrijs IS NOT NULL";

            decimal? avgPh = null;
            try
            {
                var avgList = await SqlQueryWithIndexFallback<decimal?>(avgPhWithIndex, avgPhNoIndex, pLower);
                avgPh = avgList.FirstOrDefault();
            }
            catch
            {
                var prices = items.Where(i => i.VerkoopPrijs.HasValue).Select(i => i.VerkoopPrijs!.Value).ToList();
                if (prices.Any()) avgPh = Math.Round(prices.Average(), 2);
            }

            // Average of the returned items (recent/top-10)
            decimal? avgRecent = null;
            var recentPrices = items.Where(i => i.VerkoopPrijs.HasValue).Select(i => i.VerkoopPrijs!.Value).ToList();
            if (recentPrices.Any()) avgRecent = Math.Round(recentPrices.Average(), 2);

            var result = new PriceHistoryResultDTO
            {
                Items = items,
                AverageVerkoopPrijs = avgRecent,
                OverallAverageVerkoopPrijs = avgPh
            };

            return Ok(result);
        }

        private bool ProductExists(int id) => _context.Products.Any(e => e.IdProduct == id);
        
        // Helper: probeer eerst de SQL met index-hint (kan sneller zijn), maar val terug op dezelfde query zonder hint als dit faalt
        private async Task<List<T>> SqlQueryWithIndexFallback<T>(string sqlWithIndex, string sqlWithoutIndex, params object[] parameters)
        {
            try
            {
                return await _context.Database.SqlQueryRaw<T>(sqlWithIndex, parameters).ToListAsync();
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                // Index hint faalde (index ontbreekt/permissions/SQL versie etc.); probeer zonder hint
                return await _context.Database.SqlQueryRaw<T>(sqlWithoutIndex, parameters).ToListAsync();
            }
        }

        // Admin endpoint: activeer de eerstvolgende geplande producten per locatie voor vandaag (zet status -> 3)
        [Authorize(Roles = "Veilingmeester")]
        [HttpPost("StartAuctions")]
        public async Task<ActionResult> StartAuctions ()
        {
            var today = DateTime.Today;

            var scheduledToday = await _context.Products
                .Where(p => (p.Status == 2 || p.Status == 5) && p.Datum.HasValue && p.Datum.Value.Date == today)
                .ToListAsync();

            if (!scheduledToday.Any())
                return NotFound("No auctions scheduled for today");

            var activated = new List<object>();

            var byLocation = scheduledToday.GroupBy(p => (p.Locatie ?? string.Empty));

            foreach (var group in byLocation)
            {
                var due = group.Where(p => p.Datum.HasValue)
                               .OrderBy(p => p.IdProduct)
                               .FirstOrDefault();

                var toActivate = due ?? group.OrderBy(p => p.IdProduct).FirstOrDefault();

                if (toActivate != null)
                {
                    toActivate.Status = 3;
                    _context.Entry(toActivate).State = EntityState.Modified;
                    activated.Add(new { locatie = group.Key, id = toActivate.IdProduct, startDay = toActivate.Datum });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(activated);
        }
        
        // Admin endpoint: pauzeer (status -> 5) de eerstvolgende geplande producten per locatie voor vandaag
        [Authorize(Roles = "Veilingmeester")]
        [HttpPost("PauseAuctions")]
        public async Task<ActionResult> PauseAuctions ()
        {
            var today = DateTime.Today;

            var toPause = await _context.Products
                .Where(p => p.Status == 2 && p.Datum.HasValue && p.Datum.Value.Date == today)
                .ToListAsync();

            var paused = new List<object>();

            var byLocation = toPause.GroupBy(p => (p.Locatie ?? string.Empty));

            foreach (var group in byLocation)
            {
                var due = group.Where(p => p.Datum.HasValue)
                               .OrderBy(p => p.IdProduct)
                               .FirstOrDefault();

                var toBePaused = due ?? group.OrderBy(p => p.IdProduct).FirstOrDefault();

                if (toBePaused != null)
                {
                    toBePaused.Status = 5;
                    _context.Entry(toBePaused).State = EntityState.Modified;
                    paused.Add(new { locatie = group.Key, id = toBePaused.IdProduct, startDay = toBePaused.Datum });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(paused);
        }

        // Endpoint: controleer of er producten met status 5 (gepauzeerd) bestaan
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
    }
}