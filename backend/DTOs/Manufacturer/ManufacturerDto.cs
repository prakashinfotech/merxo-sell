namespace MerxoSell.API.DTOs.Manufacturer;

public class ManufacturerDto
{
    public int      ManufacturerId { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public string?  ContactEmail   { get; set; }
    public string?  Phone          { get; set; }
    public string?  Address        { get; set; }
    public string?  Country        { get; set; }
    public string?  Website        { get; set; }
    public bool     IsActive       { get; set; }
    public DateTime CreatedAt      { get; set; }
    public int      ProductCount   { get; set; }
}
