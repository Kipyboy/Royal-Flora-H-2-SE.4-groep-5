using System.Collections.Generic;

public class VeilingSoldMatchesDTO
{
    public int ActiveProductId { get; set; }
    public string ActiveProductNaam { get; set; } = string.Empty;
    public List<SoldItemDTO> SoldProducts { get; set; } = new List<SoldItemDTO>();
    // Average verkoopPrijs for the returned sold products (null if none)
    public decimal? AverageVerkoopPrijs { get; set; }
}
