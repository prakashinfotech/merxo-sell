using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Products;

public class UpdateProductDto
{
    [Required, MaxLength(300)]
    public string   Name           { get; set; } = string.Empty;

    public string?  Description    { get; set; }

    [Range(0.01, 999999)]
    public decimal  BasePrice      { get; set; }

    public decimal? SalePrice      { get; set; }

    [Range(0, int.MaxValue)]
    public int      Stock          { get; set; }

    public bool     IsActive       { get; set; } = true;

    public int?     ManufacturerId { get; set; }
    public string?  PrimaryImageUrl { get; set; }
}
