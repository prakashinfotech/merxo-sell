namespace MerxoSell.API.DTOs.Currency;

public class CurrencyRateDto
{
    public string   CurrencyCode { get; set; } = string.Empty;
    public string   CurrencyName { get; set; } = string.Empty;
    public decimal  RateToCad    { get; set; }
    public string   Symbol       { get; set; } = string.Empty;
    public DateTime LastUpdated  { get; set; }
}
