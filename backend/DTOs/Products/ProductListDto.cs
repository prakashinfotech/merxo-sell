namespace MerxoSell.API.DTOs.Products;

public class ProductListDto
{
    public int      ProductId       { get; set; }
    public string   Name            { get; set; } = string.Empty;
    public string   Slug            { get; set; } = string.Empty;
    public decimal  BasePrice       { get; set; }
    public decimal? SalePrice       { get; set; }
    public string?  PrimaryImageUrl { get; set; }
    public decimal  Rating          { get; set; }
    public int      ReviewCount     { get; set; }
    public int      TotalSold       { get; set; }
    public bool     IsActive         { get; set; }
    public int?     ManufacturerId   { get; set; }
    public string?  ManufacturerName { get; set; }
    public string?  Description      { get; set; }
    public string?  TopComment       { get; set; }
}
