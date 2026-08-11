namespace MerxoSell.API.Models;

public class CartItem
{
    public int      CartItemId { get; set; }
    public int      CartId     { get; set; }
    public int      ProductId  { get; set; }
    public int?     VariantId  { get; set; }
    public int      Quantity   { get; set; } = 1;
    public DateTime AddedAt    { get; set; } = DateTime.UtcNow;

    public Cart            Cart    { get; set; } = null!;
    public Product         Product { get; set; } = null!;
    public ProductVariant? Variant { get; set; }
}
