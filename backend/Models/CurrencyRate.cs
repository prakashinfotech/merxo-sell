namespace MerxoSell.API.Models;

public class CurrencyRate
{
    public string   CurrencyCode { get; set; } = string.Empty;
    public string   CurrencyName { get; set; } = string.Empty;
    public decimal  RateToCad    { get; set; } = 1.000000m;
    public string   Symbol       { get; set; } = "$";
    public bool     IsActive     { get; set; } = true;
    public DateTime LastUpdated  { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = [];
}
