namespace MerxoSell.API.DTOs.Products;

public class ProductImageDto
{
    public int     ImageId   { get; set; }
    public ICollection<int> VariantIds { get; set; } = []; // Variants this image belongs to
    public string  ImageUrl  { get; set; } = string.Empty;
    public string? AltText   { get; set; }
    public bool    IsPrimary { get; set; }
    public int     SortOrder { get; set; }
}
