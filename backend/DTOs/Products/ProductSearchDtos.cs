namespace MerxoSell.API.DTOs.Products;

/// <summary>
/// Filter parameters for the global product-search endpoint.
/// All fields are optional; defaults match the public listing experience.
/// </summary>
public class ProductSearchFilterDto
{
    public string?       Q          { get; set; }
    public decimal?      MinPrice   { get; set; }
    public decimal?      MaxPrice   { get; set; }
    public int?          CategoryId { get; set; }
    public int?          SellerId   { get; set; }
    public bool?         InStock    { get; set; }
    public ProductSortBy SortBy     { get; set; } = ProductSortBy.Newest;
    public int           Page       { get; set; } = 1;
    public int           PageSize   { get; set; } = 20;
}

/// <summary>One row of an auto-suggest response.</summary>
public record SuggestItemDto(
    string Type,           // "product" or "category"
    int    Id,
    string Label,
    string? ImageUrl,
    string? Slug
);
