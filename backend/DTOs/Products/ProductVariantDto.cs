namespace MerxoSell.API.DTOs.Products;

/// <summary>
/// Variant view shown to buyers + seller dashboards. Effective unit price is
/// the Product.BasePrice + PriceDelta — frontend does the math.
/// </summary>
public class ProductVariantDto
{
    public int      VariantId  { get; set; }
    public string?  Color      { get; set; }
    public string?  Size       { get; set; }
    public decimal  PriceDelta { get; set; }
    public int      Stock      { get; set; }
    public string?  SKU        { get; set; }
    public bool     IsActive   { get; set; } = true;
    public bool     IsDefault  { get; set; }

    /// <summary>Image IDs that belong to this variant (matched against <c>ProductImageDto.ImageId</c>).</summary>
    public List<int> ImageIds  { get; set; } = [];
}
