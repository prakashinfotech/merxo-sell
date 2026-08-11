namespace MerxoSell.API.DTOs.Products;

public class ProductDetailDto
{
    public int                    ProductId     { get; set; }
    public string                 Name          { get; set; } = string.Empty;
    public string                 Slug          { get; set; } = string.Empty;
    public string?                Description   { get; set; }
    public decimal                BasePrice     { get; set; }
    public decimal?               SalePrice     { get; set; }
    public int                    Stock         { get; set; }
    public bool                   IsActive      { get; set; }
    public int                    CategoryId    { get; set; }
    public string                 CategoryName  { get; set; } = string.Empty;
    public int?     ManufacturerId  { get; set; }
    public string?  ManufacturerName{ get; set; }
    public DateTime               CreatedAt     { get; set; }
    public List<ProductImageDto>   Images        { get; set; } = [];
    public List<string>            Colors        { get; set; } = [];
    public List<string>            Sizes         { get; set; } = [];
    public List<ProductVariantDto> Variants      { get; set; } = [];
    /// <summary>True when the product's category is flagged as fashion (drives variant UI on the storefront).</summary>
    public bool                    IsFashion     { get; set; }
    public RatingSummaryDto        RatingSummary { get; set; } = new();

    public int InCartCount    { get; set; }
    public int AvailableStock { get; set; }
}
