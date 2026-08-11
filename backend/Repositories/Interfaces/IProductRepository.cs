using MerxoSell.API.DTOs.Products;
using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IProductRepository
{
    /// <summary>
    /// Returns a page of projected list DTOs with TotalSold computed via
    /// correlated subquery. All prices are in CAD — no conversion is applied.
    /// </summary>
    Task<(IReadOnlyList<ProductListDto> Items, int TotalCount)> GetPagedAsync(ProductFilterDto filter);

    /// <summary>
    /// Returns the full product entity with Images, Variants, Reviews, and
    /// Category loaded. Returns null when not found or inactive.
    /// </summary>
    Task<Product?> GetDetailAsync(int productId);

    /// <summary>Returns top N products sorted by TotalSold.</summary>
    Task<IReadOnlyList<ProductListDto>> GetBestSellersAsync(int count);

    /// <summary>Returns total quantity of a product currently held in active carts.</summary>
    Task<int> GetInCartCountAsync(int productId);

    /// <summary>
    /// Global search with fuzzy match on Name + Description, plus full filtering.
    /// Only Approved + Active + non-deleted rows are returned.
    /// </summary>
    Task<(IReadOnlyList<ProductListDto> Items, int TotalCount)> SearchAsync(ProductSearchFilterDto filter);

    /// <summary>
    /// Auto-suggest: top product names and category names matching q.
    /// Returns up to 8 combined results (5 products + 3 categories).
    /// </summary>
    Task<IReadOnlyList<SuggestItemDto>> SuggestAsync(string query);
}
