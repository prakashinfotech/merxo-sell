using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class SellerRepository : ISellerRepository
{
    private readonly AppDbContext _db;
    public SellerRepository(AppDbContext db) => _db = db;

    public async Task<Seller?> GetByIdAsync(int sellerId) =>
        await _db.Sellers
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && !s.IsDeleted);

    public async Task<Seller?> GetByUserIdAsync(int userId) =>
        await _db.Sellers
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

    public async Task<IEnumerable<Seller>> GetAllAsync(string? search, bool? isActive)
    {
        var q = _db.Sellers.AsNoTracking()
            .Include(s => s.User)
            .Where(s => !s.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(s => s.StoreName.Contains(search) || s.User.Email.Contains(search));

        if (isActive.HasValue)
            q = q.Where(s => s.IsActive == isActive.Value);

        return await q.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    public async Task<Seller> CreateAsync(Seller seller)
    {
        _db.Sellers.Add(seller);
        await _db.SaveChangesAsync();
        return seller;
    }

    public async Task<Seller> UpdateAsync(Seller seller)
    {
        seller.UpdatedAt = DateTime.UtcNow;
        _db.Sellers.Update(seller);
        await _db.SaveChangesAsync();
        return seller;
    }

    public async Task<bool> SetStatusAsync(int sellerId, bool isActive)
    {
        var seller = await _db.Sellers.FindAsync(sellerId);
        if (seller is null) return false;
        seller.IsActive  = isActive;
        seller.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByUserIdAsync(int userId) =>
        await _db.Sellers.AnyAsync(s => s.UserId == userId && !s.IsDeleted);

    public async Task<bool> SoftDeleteAsync(int sellerId)
    {
        var seller = await _db.Sellers.FindAsync(sellerId);
        if (seller is null) return false;
        
        seller.IsDeleted = true;
        seller.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
