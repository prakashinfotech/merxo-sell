using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class SellerProductRepository : ISellerProductRepository
{
    private readonly AppDbContext _db;
    public SellerProductRepository(AppDbContext db) => _db = db;

    private IQueryable<Product> BaseQuery() =>
        _db.Products
           .AsNoTracking()
           .Where(p => !p.IsDeleted)
           .Include(p => p.Category)
           .Include(p => p.Manufacturer)
           .Include(p => p.Seller);

    public async Task<IEnumerable<Product>> GetBySellerAsync(int sellerId, string? search, string? status)
    {
        var q = BaseQuery()
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Where(p => p.SellerId == sellerId);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.Contains(search));

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(p => p.Status == status);

        return await q.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Product?> GetByIdAndSellerAsync(int productId, int sellerId) =>
        await _db.Products
            .Where(p => p.ProductId == productId && p.SellerId == sellerId && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Include(p => p.Seller)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Images)
                .ThenInclude(i => i.Variants)
            .Include(p => p.Colors)
            .Include(p => p.Sizes)
            .FirstOrDefaultAsync();

    public async Task<Product> CreateAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<bool> SoftDeleteAsync(int productId, int sellerId)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.SellerId == sellerId && !p.IsDeleted);
        if (product is null) return false;
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Product>> GetPendingAsync() =>
        await BaseQuery()
            .Where(p => p.Status == "Pending" && p.IsActive)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Product?> GetByIdForApprovalAsync(int productId) =>
        await BaseQuery()
            .Include(p => p.ApprovalLogs.OrderByDescending(l => l.CreatedAt).Take(1))
            .FirstOrDefaultAsync(p => p.ProductId == productId);
}
