namespace MerxoSell.API.DTOs.Products;

public enum ProductSortBy
{
    Newest,
    PriceAsc,
    PriceDesc,
    Rating,
    BestSelling
}

public class ProductFilterDto
{
    public int?           CategoryId { get; set; }
    public string?        Search     { get; set; }
    public decimal?       MinPrice   { get; set; }
    public decimal?       MaxPrice   { get; set; }
    public ProductSortBy  SortBy     { get; set; } = ProductSortBy.Newest;
    public int            Page       { get; set; } = 1;
    public int            PageSize   { get; set; } = 20;
}
