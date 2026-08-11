using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(int couponId);
    Task<Coupon?> GetByCodeAsync(string couponCode);
    Task<IReadOnlyList<Coupon>> GetAllAsync(bool includeInactive = false);
    Task<IReadOnlyList<Coupon>> GetAvailableAsync(DateTime now);
    Task<Coupon> CreateAsync(Coupon coupon);
    Task SaveChangesAsync();
    Task<bool> HasUserUsedCouponAsync(int couponId, int userId);
    Task RecordUsageAsync(CouponUsageHistory usage);
    Task DeleteAsync(Coupon coupon);
}
