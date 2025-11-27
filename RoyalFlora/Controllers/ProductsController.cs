using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI;
using RoyalFlora.Migrations;

namespace RoyalFlora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private VeilingDTO VeilingProductInLaden(Product product)
        {
            return new VeilingDTO
            {
                id = product.IdProduct,
                naam = product.ProductNaam ?? string.Empty,
                beschrijving = product.ProductBeschrijving ?? string.Empty,
                locatie = product.Locatie.ToString() ?? string.Empty,
                status = product.Status ?? 0,
            };
        }

        [HttpGet("Veiling")]
        public async Task<ActionResult<IEnumerable<VeilingDTO>>> GetVeilingProducts()
        {
            var products = await _context.Products.ToListAsync();
            var VeilingProducten = products.Select(VeilingProductInLaden).ToList();
            return VeilingProducten;
        }

        private ClockDTO PrijsVoorKlok(Product product)
        {
            return new ClockDTO
            {
                minimumPrijs = float.TryParse(product.MinimumPrijs, out var price) ? price : 0f,
                locatie = product.Locatie.ToString() ?? string.Empty,
                status = product.Status
            };
        }

        [HttpGet("Klok")]
        public async Task<ActionResult<ClockDTO>> GetKlokPrijs([FromQuery] string locatie)
        {
            var product = await _context.Products
                .Where(p => p.Status == 3 &&
                (p.Locatie ?? "").ToLower() == locatie.ToLower())
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return new ClockDTO
            {
                minimumPrijs = float.TryParse(product.MinimumPrijs, out var price) ? price : 0f,
                locatie = product.Locatie,
                status = product.Status
            };
        }


        private readonly MyDbContext _context;

        public ProductsController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPatch("{id:int}/koop")]
        public async Task<IActionResult> KoopProduct(int id, [FromBody] KoopDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();


            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            product.Status = 4;
            product.Koper = userId.Value;
            product.verkoopPrijs = dto.verkoopPrijs;

            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // GET: api/Products1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts([FromQuery] string? location)
        {
            List<Product> products = await _context.Products
                .Include(p => p.LeverancierNavigation)
                .Include(p => p.StatusNavigation)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(location))
            {
                products = products
                    .Where(p => (p.Locatie?.ToString() ?? string.Empty)
                        .Equals(location, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            List<ProductDTO> productDTOs = new List<ProductDTO>();

            foreach (Product product in products)
            {
                string leverancierNaam = product.LeverancierNavigation?.BedrijfNaam ?? string.Empty;
                string datum = product.Datum?.ToString("yyyy-MM-dd") ?? string.Empty;
                string locatie = product.Locatie?.ToString() ?? string.Empty;
                string status = product.StatusNavigation?.Beschrijving ?? string.Empty; // map int FK to beschrijving text

                var dto = new ProductDTO
                {
                    id = product.IdProduct,
                    naam = product.ProductNaam ?? string.Empty,
                    merk = leverancierNaam,
                    prijs = product.MinimumPrijs ?? string.Empty,
                    datum = datum,
                    locatie = locatie,
                    status = status,
                    aantal = product.Aantal
                };
                productDTOs.Add(dto);
            }


            // Debug output
            foreach (var dto in productDTOs)
            {
                Console.WriteLine($"ProductDTO - Id: {dto.id}, Naam: {dto.naam}, Merk: {dto.merk}, Prijs: {dto.prijs}, Datum: {dto.datum}, Locatie: {dto.locatie}, Status: {dto.status}");
            }
            return productDTOs;
        }



        // GET: api/Products1/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products1/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.IdProduct)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products1
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromForm] Product product, [FromForm] List<IFormFile> images)
        {
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                images = new List<IFormFile>();

                _context.Fotos.AddRange(images.Select(image => new Foto
                {
                    IdProduct = product.IdProduct,
                    FotoPath = image.FileName
                }));

                if (images.IsNullOrEmpty())
                {
                    Console.WriteLine("het werkt niet lol");
                }

                await _context.SaveChangesAsync();
                return Ok(product);
            _context.Fotos.AddRange(images.Select(image => new Foto
            {
                IdProduct = product.IdProduct,
                FotoPath = image.FileName 
            }));


            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.IdProduct }, new ResponseDTO { naam = product.ProductNaam ?? string.Empty, bericht = "Product succesvol geregistreerd!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Registratie mislukt" });
            }
        }

        // DELETE: api/Products1/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.IdProduct == id);
        }

        [HttpPost("Advance")]
        public async Task<IActionResult> Advance([FromQuery] string locatie)
        {
            var current = await _context.Products
                .Where(p => p.Status == 3 && (p.Locatie ?? "") == locatie)
                .FirstOrDefaultAsync();

            if (current == null)
                return NotFound("No active product found");

            // Mark current as finished
            current.Status = 5;
            _context.Entry(current).State = EntityState.Modified;

            // Find next product
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


    }



}
