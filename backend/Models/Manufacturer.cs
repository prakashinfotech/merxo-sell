namespace MerxoSell.API.Models;

public class Manufacturer
{
    public int       ManufacturerId { get; set; }
    public string    Name           { get; set; } = string.Empty;
    public string?   ContactEmail   { get; set; }
    public string?   Phone          { get; set; }
    public string?   Address        { get; set; }
    public string?   Country        { get; set; }
    public string?   Website        { get; set; }
    public bool      IsActive       { get; set; } = true;
    public DateTime  CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt      { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
