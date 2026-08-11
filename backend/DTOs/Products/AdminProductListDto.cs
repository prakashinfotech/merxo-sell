namespace MerxoSell.API.DTOs.Products;

public class AdminProductListDto
{
    public int      ProductId        { get; set; }
    public string   Name             { get; set; } = string.Empty;
    public string   Slug             { get; set; } = string.Empty;
    public decimal  BasePrice        { get; set; }
    public decimal? SalePrice        { get; set; }
    public int      Stock            { get; set; }
    public bool     IsActive         { get; set; }
    public string   CategoryName     { get; set; } = string.Empty;
    public int?     ManufacturerId   { get; set; }
    public string?  ManufacturerName { get; set; }
    public string?  PrimaryImageUrl  { get; set; }
    public DateTime CreatedAt        { get; set; }
}
