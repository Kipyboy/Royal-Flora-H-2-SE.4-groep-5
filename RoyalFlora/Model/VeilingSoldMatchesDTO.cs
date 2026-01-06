using System.Collections.Generic;

public class VeilingSoldMatchesDTO
{
    public int ActiveProductId { get; set; }
    public string ActiveProductNaam { get; set; } = string.Empty;
    public List<SoldItemDTO> SoldProducts { get; set; } = new List<SoldItemDTO>();
}
