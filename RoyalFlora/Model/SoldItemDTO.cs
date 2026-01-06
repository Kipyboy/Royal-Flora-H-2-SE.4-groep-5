public class SoldItemDTO
{
    public int IdProduct { get; set; }
    public string ProductNaam { get; set; } = string.Empty;
    public decimal VerkoopPrijs { get; set; }
    public int? Aantal { get; set; }
}
