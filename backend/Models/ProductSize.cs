namespace MerxoSell.API.Models;

public class ProductSize
{
    public int     SizeId    { get; set; }
    public int     ProductId { get; set; }
    public string  SizeName  { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
