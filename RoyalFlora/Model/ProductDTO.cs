public class ProductDTO
{
    
    public int id { get; set; }
    public string naam { get; set; } = string.Empty;
    public string merk { get; set; } = string.Empty; 
    public string prijs { get; set; } = string.Empty; 
    public string datum { get; set; } = string.Empty; 
    public string locatie { get; set; } = string.Empty; 
    public string status { get; set; } = string.Empty; 
    public int? aantal { get; set; }
    
    public string? fotoPath { get; set; } = string.Empty;
}