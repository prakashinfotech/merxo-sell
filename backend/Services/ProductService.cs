using MerxoSell.API.DTOs.Common;
using MerxoSell.API.DTOs.Products;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<PagedResult<ProductListDto>> GetPagedAsync(ProductFilterDto filter)
    {
        filter.Page     = Math.Max(1, filter.Page);
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);

        var (items, totalCount) = await _repo.GetPagedAsync(filter);

        return new PagedResult<ProductListDto>
        {
            Data = items,
            Pagination = new PaginationMeta
            {
                Page       = filter.Page,
                PageSize   = filter.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            }
        };
    }

    public async Task<ProductDetailDto?> GetDetailAsync(int productId)
    {
        var product     = await _repo.GetDetailAsync(productId);
        if (product is null) return null;

        var inCartCount = await _repo.GetInCartCountAsync(productId);
        return MapDetail(product, inCartCount);
    }

    public async Task<IReadOnlyList<ProductListDto>> GetBestSellersAsync(int count)
    {
        return await _repo.GetBestSellersAsync(count);
    }

    public async Task<PagedResult<ProductListDto>> SearchAsync(ProductSearchFilterDto filter)
    {
        filter.Page     = Math.Max(1, filter.Page);
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);

        var (items, totalCount) = await _repo.SearchAsync(filter);

        return new PagedResult<ProductListDto>
        {
            Data = items,
            Pagination = new PaginationMeta
            {
                Page       = filter.Page,
                PageSize   = filter.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            }
        };
    }

    public async Task<IReadOnlyList<SuggestItemDto>> SuggestAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            return Array.Empty<SuggestItemDto>();
        return await _repo.SuggestAsync(query);
    }

    private ProductDetailDto MapDetail(Product p, int inCartCount) => new()
    {
        ProductId    = p.ProductId,
        Name         = p.Name,
        Slug         = p.Slug,
        Description  = p.Description,
        BasePrice    = p.BasePrice,
        SalePrice    = p.SalePrice,
        Stock        = p.Stock,
        InCartCount  = inCartCount,
        AvailableStock = Math.Max(0, p.Stock - inCartCount),
        IsActive     = p.IsActive,
        CategoryId       = p.CategoryId,
        CategoryName     = p.Category.Name,
        IsFashion        = p.Category.IsFashion,
        ManufacturerId   = p.ManufacturerId,
        ManufacturerName = p.Manufacturer?.Name,
        CreatedAt        = p.CreatedAt,
        // Distinct colour / size labels are derived from the active variant
        // matrix — the source of truth — so the swatch + size rows stay in
        // sync with what the seller actually entered.
        Colors = p.Variants.Where(v => v.IsActive && !string.IsNullOrWhiteSpace(v.Color))
                           .Select(v => v.Color!).Distinct().ToList(),
        Sizes  = p.Variants.Where(v => v.IsActive && !string.IsNullOrWhiteSpace(v.Size))
                           .Select(v => v.Size!).Distinct().ToList(),
        Images           = p.Images.Select(i => new ProductImageDto
        {
            ImageId    = i.ImageId,
            VariantIds = i.Variants.Select(v => v.VariantId).ToList(),
            ImageUrl   = i.ImageUrl,
            AltText    = i.AltText,
            IsPrimary  = i.IsPrimary,
            SortOrder  = i.SortOrder
        }).ToList(),
        Variants = p.Variants.Where(v => v.IsActive).Select(v => new ProductVariantDto
        {
            VariantId  = v.VariantId,
            Color      = v.Color,
            Size       = v.Size,
            PriceDelta = v.PriceDelta,
            Stock      = v.Stock,
            SKU        = v.SKU,
            IsActive   = v.IsActive,
            IsDefault  = v.IsDefault,
            // Pre-compute the image-id list so the frontend can swap photos
            // by colour without an extra round-trip.
            ImageIds   = v.Images.Select(img => img.ImageId).ToList(),
        }).ToList(),
        RatingSummary = BuildRatingSummary(p.Reviews)
    };

    private static RatingSummaryDto BuildRatingSummary(ICollection<Review> reviews)
    {
        if (reviews.Count == 0)
            return new RatingSummaryDto();

        var distribution = Enumerable.Range(1, 5)
            .ToDictionary(star => star, star => reviews.Count(r => r.Rating == star));

        return new RatingSummaryDto
        {
            AvgRating    = Math.Round((decimal)reviews.Average(r => r.Rating), 1),
            ReviewCount  = reviews.Count,
            Distribution = distribution
        };
    }
}
