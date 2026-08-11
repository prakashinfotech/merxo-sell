using MerxoSell.API.DTOs.Common;
using MerxoSell.API.DTOs.Products;

namespace MerxoSell.API.Services.Interfaces;

public interface IProductService
{
    /// <summary>Returns a paginated, filtered list of products. All prices are in CAD.</summary>
    Task<PagedResult<ProductListDto>> GetPagedAsync(ProductFilterDto filter);

    /// <summary>Returns full product detail. Returns null when not found or inactive.</summary>
    Task<ProductDetailDto?> GetDetailAsync(int productId);

    /// <summary>Returns top N best selling products.</summary>
    Task<IReadOnlyList<ProductListDto>> GetBestSellersAsync(int count);

    /// <summary>Global search with fuzzy match + filters + sort + paging.</summary>
    Task<PagedResult<ProductListDto>> SearchAsync(ProductSearchFilterDto filter);

    /// <summary>Auto-suggest results for the navbar.</summary>
    Task<IReadOnlyList<SuggestItemDto>> SuggestAsync(string query);
}
