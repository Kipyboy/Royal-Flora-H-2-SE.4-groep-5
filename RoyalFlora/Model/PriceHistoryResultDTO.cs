using System.Collections.Generic;

public class PriceHistoryResultDTO
{
    public List<PriceHistoryItemDTO> Items { get; set; } = new List<PriceHistoryItemDTO>();
    public decimal? AverageVerkoopPrijs { get; set; }
}
