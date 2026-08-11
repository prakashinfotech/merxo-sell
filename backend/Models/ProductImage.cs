namespace MerxoSell.API.Models;

public class ProductImage
{
    public int     ImageId   { get; set; }
    public int     ProductId { get; set; }
    /// <summary>
    /// When set, this image is shown for a specific variant (e.g. "Red"
    /// colour photos). When null, the image is a product-level image that
    /// renders regardless of variant selection.
    /// </summary>
    public string  ImageUrl  { get; set; } = string.Empty;
    public string? AltText   { get; set; }
    public bool    IsPrimary { get; set; }
    public int     SortOrder { get; set; }

    public Product         Product  { get; set; } = null!;
    public ICollection<ProductVariant> Variants { get; set; } = [];
}
