using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
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

        // GET: api/Products1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            List<Product> products = await _context.Products
            .Include(p => p.LeverancierNavigation)
            .ToListAsync();
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
        [HttpGet("{id}")]
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
        [HttpPut("{id}")]
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
        public async Task<ActionResult<Product>> PostProduct([FromForm]Product product, [FromForm] List<IFormFile> images)
        {
            try
            {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _context.Fotos.AddRange(images.Select(image => new Foto
            {
                IdProduct = product.IdProduct,
                FotoPath = image.FileName 
            }));

            await _context.SaveChangesAsync();
            return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Registratie mislukt" });
            }
        }

        // DELETE: api/Products1/5
        [HttpDelete("{id}")]
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
    }
    
}
