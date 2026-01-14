public class SoldItemDTO
{
    public int IdProduct { get; set; }
    public string? ProductNaam { get; set; }
    public decimal? VerkoopPrijs { get; set; }
    public int? Aantal { get; set; }
    // Optional: formatted sold date (dd-MM-yyyy)
    public string? SoldDate { get; set; }
    // Optional: name of the aanvoerder (supplier/company)
    public string? AanvoerderNaam { get; set; }
}
