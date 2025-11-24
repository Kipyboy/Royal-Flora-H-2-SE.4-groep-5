public class ProductDTO
{
    // NOTE: System.Text.Json (default in ASP.NET Core) serializes public properties, NOT public fields.
    // Previously these were public fields, causing empty JSON objects ({}). Converting to properties fixes serialization.
    public int id { get; set; }
    public string naam { get; set; } = string.Empty;
    public string merk { get; set; } = string.Empty; 
    public string prijs { get; set; } = string.Empty; 
    public string datum { get; set; } = string.Empty; 
    public string locatie { get; set; } = string.Empty; 
    public string status { get; set; } = string.Empty; 
}