namespace MerxoSell.API.Models;

public class ProductVariant
{
    public int     VariantId  { get; set; }
    public int     ProductId  { get; set; }
    public string? Color      { get; set; }
    public string? Size       { get; set; }
    /// <summary>Δ on top of the product BasePrice (CAD). Positive or negative.</summary>
    public decimal PriceDelta { get; set; }
    public int     Stock      { get; set; }
    public string? SKU        { get; set; }
    public bool    IsActive   { get; set; } = true;
    /// <summary>The variant pre-selected on the product detail page.</summary>
    public bool    IsDefault  { get; set; }

    public Product                       Product { get; set; } = null!;
    /// <summary>
    /// Images that belong specifically to this variant (e.g. product photos
    /// taken in the variant's colour). When empty, the buyer UI falls back
    /// to the product-level images.
    /// </summary>
    public ICollection<ProductImage>     Images  { get; set; } = [];
}
