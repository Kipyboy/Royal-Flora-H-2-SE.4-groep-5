using Microsoft.AspNetCore.Mvc;

namespace RoyalFlora.Controllers;

using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public ProductsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // Path to frontend public folder
        var publicFolder = Path.Combine(_env.ContentRootPath, "..", "royal-flora-frontend", "public");
        var filePath = Path.Combine(publicFolder, "products.json");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("products.json not found");
        }
        var json = System.IO.File.ReadAllText(filePath);
        var products = JsonSerializer.Deserialize<object>(json);
        return Ok(products);
    }
}
    
