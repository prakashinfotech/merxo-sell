using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Products;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<ProductListDto> Items, int TotalCount)> GetPagedAsync(
        ProductFilterDto filter)
    {
        // ── Build base query ─────────────────────────────────────────
        // Buyers must only see Approved products; Pending/Rejected listings stay hidden.
        var query = _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted && p.Status == "Approved")
            .AsQueryable();

        // ── Filters ───────────────────────────────────────────────────
        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search));

        // Effective price = SalePrice when set, otherwise BasePrice
        if (filter.MinPrice.HasValue)
            query = query.Where(p => (p.SalePrice ?? p.BasePrice) >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => (p.SalePrice ?? p.BasePrice) <= filter.MaxPrice.Value);

        // ── Sorting ───────────────────────────────────────────────────
        query = filter.SortBy switch
        {
            ProductSortBy.PriceAsc    => query.OrderBy(p => p.SalePrice ?? p.BasePrice),
            ProductSortBy.PriceDesc   => query.OrderByDescending(p => p.SalePrice ?? p.BasePrice),
            ProductSortBy.Rating      => query.OrderByDescending(p =>
                                             p.Reviews.Average(r => (double?)r.Rating) ?? 0.0),
            ProductSortBy.BestSelling => query.OrderByDescending(p =>
                                             _db.OrderItems
                                                .Where(oi => oi.ProductId == p.ProductId)
                                                .Sum(oi => (int?)oi.Quantity) ?? 0),
            _                         => query.OrderByDescending(p => p.CreatedAt) // Newest
        };

        // ── Count before paging ───────────────────────────────────────
        var totalCount = await query.CountAsync();

        // ── Project + paginate ────────────────────────────────────────
        // Projection is done at the DB level to avoid over-fetching navigation
        // collections. TotalSold uses a correlated subquery on OrderItems.
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductListDto
            {
                ProductId       = p.ProductId,
                Name            = p.Name,
                Slug            = p.Slug,
                BasePrice       = p.BasePrice,
                SalePrice       = p.SalePrice,
                PrimaryImageUrl = p.Images
                                   .Where(i => i.IsPrimary)
                                   .Select(i => i.ImageUrl)
                                   .FirstOrDefault(),
                Rating          = (decimal?)p.Reviews.Average(r => (double?)r.Rating) ?? 0m,
                ReviewCount     = p.Reviews.Count(),
                TotalSold       = _db.OrderItems
                                     .Where(oi => oi.ProductId == p.ProductId)
                                     .Sum(oi => (int?)oi.Quantity) ?? 0,
                IsActive        = p.IsActive,
                Description     = p.Description,
                TopComment      = p.Reviews
                                     .Where(r => r.Status == "Approved" && r.Comment != null && r.Comment != "")
                                     .OrderByDescending(r => r.Rating)
                                     .ThenByDescending(r => r.CreatedAt)
                                     .Select(r => r.Comment)
                                     .FirstOrDefault()
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Product?> GetDetailAsync(int productId)
        => await _db.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Manufacturer)
                    .Include(p => p.Images.OrderBy(i => i.SortOrder))
                        .ThenInclude(i => i.Variants)
                    .Include(p => p.Colors)
                    .Include(p => p.Sizes)
                    .Include(p => p.Variants.Where(v => v.IsActive).OrderBy(v => v.PriceDelta))
                        .ThenInclude(v => v.Images)
                    .Include(p => p.Reviews.Where(r => r.Status == "Approved"))
                    .FirstOrDefaultAsync(p => p.ProductId == productId
                                              && p.IsActive
                                              && !p.IsDeleted
                                              && p.Status == "Approved");

    public async Task<IReadOnlyList<ProductListDto>> GetBestSellersAsync(int count)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted && p.Status == "Approved")
            .OrderByDescending(p => _db.OrderItems
                .Where(oi => oi.ProductId == p.ProductId)
                .Sum(oi => (int?)oi.Quantity) ?? 0)
            .Take(count)
            .Select(p => new ProductListDto
            {
                ProductId       = p.ProductId,
                Name            = p.Name,
                Slug            = p.Slug,
                BasePrice       = p.BasePrice,
                SalePrice       = p.SalePrice,
                PrimaryImageUrl = p.Images
                                   .Where(i => i.IsPrimary)
                                   .Select(i => i.ImageUrl)
                                   .FirstOrDefault(),
                Rating          = (decimal?)p.Reviews.Average(r => (double?)r.Rating) ?? 0m,
                ReviewCount     = p.Reviews.Count(),
                TotalSold       = _db.OrderItems
                                     .Where(oi => oi.ProductId == p.ProductId)
                                     .Sum(oi => (int?)oi.Quantity) ?? 0,
                IsActive        = p.IsActive,
                Description     = p.Description,
                TopComment      = p.Reviews
                                     .Where(r => r.Status == "Approved" && r.Comment != null && r.Comment != "")
                                     .OrderByDescending(r => r.Rating)
                                     .ThenByDescending(r => r.CreatedAt)
                                     .Select(r => r.Comment)
                                     .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<int> GetInCartCountAsync(int productId)
    {
        return await _db.CartItems
            .AsNoTracking()
            .Where(ci => ci.ProductId == productId)
            .SumAsync(ci => (int?)ci.Quantity) ?? 0;
    }

    public async Task<(IReadOnlyList<ProductListDto> Items, int TotalCount)> SearchAsync(ProductSearchFilterDto filter)
    {
        // Buyers only see Approved, active, non-deleted products.
        var query = _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted && p.Status == "Approved");

        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var pattern = $"%{filter.Q.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name, pattern) ||
                (p.Description != null && EF.Functions.Like(p.Description, pattern)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.SellerId.HasValue)
            query = query.Where(p => p.SellerId == filter.SellerId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => (p.SalePrice ?? p.BasePrice) >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => (p.SalePrice ?? p.BasePrice) <= filter.MaxPrice.Value);

        if (filter.InStock == true)
            query = query.Where(p => p.Stock > 0);

        query = filter.SortBy switch
        {
            ProductSortBy.PriceAsc    => query.OrderBy(p => p.SalePrice ?? p.BasePrice),
            ProductSortBy.PriceDesc   => query.OrderByDescending(p => p.SalePrice ?? p.BasePrice),
            ProductSortBy.Rating      => query.OrderByDescending(p =>
                                             p.Reviews.Where(r => r.Status == "Approved")
                                                      .Average(r => (double?)r.Rating) ?? 0.0),
            ProductSortBy.BestSelling => query.OrderByDescending(p =>
                                             _db.OrderItems
                                                .Where(oi => oi.ProductId == p.ProductId)
                                                .Sum(oi => (int?)oi.Quantity) ?? 0),
            _                         => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductListDto
            {
                ProductId       = p.ProductId,
                Name            = p.Name,
                Slug            = p.Slug,
                BasePrice       = p.BasePrice,
                SalePrice       = p.SalePrice,
                PrimaryImageUrl = p.Images
                                   .Where(i => i.IsPrimary)
                                   .Select(i => i.ImageUrl)
                                   .FirstOrDefault(),
                Rating          = (decimal?)p.Reviews.Where(r => r.Status == "Approved")
                                                     .Average(r => (double?)r.Rating) ?? 0m,
                ReviewCount     = p.Reviews.Count(r => r.Status == "Approved"),
                TotalSold       = _db.OrderItems
                                     .Where(oi => oi.ProductId == p.ProductId)
                                     .Sum(oi => (int?)oi.Quantity) ?? 0,
                IsActive        = p.IsActive,
                Description     = p.Description,
                TopComment      = p.Reviews
                                     .Where(r => r.Status == "Approved" && r.Comment != null && r.Comment != "")
                                     .OrderByDescending(r => r.Rating)
                                     .ThenByDescending(r => r.CreatedAt)
                                     .Select(r => r.Comment)
                                     .FirstOrDefault()
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<SuggestItemDto>> SuggestAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<SuggestItemDto>();
        var pattern = $"%{query.Trim()}%";

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted && p.Status == "Approved" &&
                        EF.Functions.Like(p.Name, pattern))
            .OrderByDescending(p => p.ViewCount)
            .Take(5)
            .Select(p => new SuggestItemDto(
                "product",
                p.ProductId,
                p.Name,
                p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault(),
                p.Slug))
            .ToListAsync();

        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && EF.Functions.Like(c.Name, pattern))
            .OrderBy(c => c.Name)
            .Take(3)
            .Select(c => new SuggestItemDto(
                "category",
                c.CategoryId,
                c.Name,
                null,
                c.Slug))
            .ToListAsync();

        return products.Concat(categories).Take(8).ToList();
    }
}
