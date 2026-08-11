namespace MerxoSell.API.Models;

public class OrderItem
{
    public int     OrderItemId  { get; set; }
    public int     OrderId      { get; set; }
    public int     ProductId    { get; set; }
    public int?    VariantId    { get; set; }
    public string  ProductName  { get; set; } = string.Empty;
    public string? VariantInfo  { get; set; }
    public int     Quantity     { get; set; }
    // Price in CAD locked at the moment the order was placed
    public decimal UnitPriceCAD { get; set; }

    public Order           Order   { get; set; } = null!;
    public Product         Product { get; set; } = null!;
    public ProductVariant? Variant { get; set; }
}
