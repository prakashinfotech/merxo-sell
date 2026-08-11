namespace MerxoSell.API.Models;

public class ProductColor
{
    public int     ColorId   { get; set; }
    public int     ProductId { get; set; }
    public string  ColorName { get; set; } = string.Empty;
    public string? HexCode   { get; set; }

    public Product Product { get; set; } = null!;
}
