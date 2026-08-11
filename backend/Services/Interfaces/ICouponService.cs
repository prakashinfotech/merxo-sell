using MerxoSell.API.DTOs.Coupons;

namespace MerxoSell.API.Services.Interfaces;

public interface ICouponService
{
    Task<IReadOnlyList<CouponDto>> GetAllAsync();
    Task<IReadOnlyList<CouponSummaryDto>> GetAvailableAsync();
    Task<CouponDto> CreateAsync(CreateCouponDto dto);
    Task<CouponDto> UpdateAsync(int couponId, UpdateCouponDto dto);
    Task<CouponDto> SetActiveAsync(int couponId, bool isActive);
    Task DeleteAsync(int couponId);
    Task<CouponValidationResultDto> ValidateAsync(string couponCode, decimal orderAmount, int? userId = null);
    Task<IReadOnlyList<CouponUsageStatDto>> GetUsageStatsAsync();
    Task RecordUsageAsync(string couponCode, int userId, int orderId, decimal orderAmount, decimal discountAmount);
}
