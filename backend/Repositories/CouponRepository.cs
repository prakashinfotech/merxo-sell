using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _db;

    public CouponRepository(AppDbContext db) => _db = db;

    public async Task<Coupon?> GetByIdAsync(int couponId)
        => await _db.Coupons.FirstOrDefaultAsync(c => c.CouponId == couponId && !c.IsDeleted);

    public async Task<Coupon?> GetByCodeAsync(string couponCode)
    {
        var normalized = couponCode.Trim().ToUpperInvariant();
        return await _db.Coupons
            .FirstOrDefaultAsync(c => c.CouponCode == normalized && !c.IsDeleted);
    }

    public async Task<IReadOnlyList<Coupon>> GetAllAsync(bool includeInactive = false)
        => await _db.Coupons
            .AsNoTracking()
            .Where(c => !c.IsDeleted && (includeInactive || c.IsActive))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyList<Coupon>> GetAvailableAsync(DateTime now)
        => await _db.Coupons
            .AsNoTracking()
            .Where(c => !c.IsDeleted
                && c.IsActive
                && c.StartDate <= now
                && (c.ExpiryDate == null || c.ExpiryDate >= now)
                && (c.UsageLimit == null || c.UsedCount < c.UsageLimit))
            .OrderBy(c => c.ExpiryDate ?? DateTime.MaxValue)
            .ThenBy(c => c.MinimumPurchaseAmount ?? 0)
            .ToListAsync();

    public async Task<Coupon> CreateAsync(Coupon coupon)
    {
        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();
        return coupon;
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task<bool> HasUserUsedCouponAsync(int couponId, int userId)
        => await _db.CouponUsageHistory
            .AnyAsync(h => h.CouponId == couponId && h.UserId == userId);

    public async Task RecordUsageAsync(CouponUsageHistory usage)
    {
        _db.CouponUsageHistory.Add(usage);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Coupon coupon)
    {
        coupon.IsDeleted = true;
        coupon.IsActive = false;
        coupon.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
