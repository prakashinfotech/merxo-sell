namespace MerxoSell.API.DTOs.Products;

public class RatingSummaryDto
{
    public decimal              AvgRating    { get; set; }
    public int                  ReviewCount  { get; set; }
    public Dictionary<int, int> Distribution { get; set; } = [];
}
