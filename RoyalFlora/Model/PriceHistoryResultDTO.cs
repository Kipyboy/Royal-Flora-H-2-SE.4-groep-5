using System.Collections.Generic;

public class PriceHistoryResultDTO
{
    public List<PriceHistoryItemDTO> Items { get; set; } = new List<PriceHistoryItemDTO>();
    // Average of the returned items (e.g. top 10 recent)
    public decimal? AverageVerkoopPrijs { get; set; }
    // Average across ALL sold items with the same product name (not limited to returned items)
    public decimal? OverallAverageVerkoopPrijs { get; set; }
}
