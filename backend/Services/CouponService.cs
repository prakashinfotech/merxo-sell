using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Coupons;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class CouponService : ICouponService
{
    private readonly ICouponRepository _couponRepo;
    private readonly AppDbContext _db;

    public CouponService(ICouponRepository couponRepo, AppDbContext db)
    {
        _couponRepo = couponRepo;
        _db = db;
    }

    public async Task<IReadOnlyList<CouponDto>> GetAllAsync()
        => (await _couponRepo.GetAllAsync(includeInactive: true)).Select(Map).ToList();

    public async Task<IReadOnlyList<CouponSummaryDto>> GetAvailableAsync()
        => (await _couponRepo.GetAvailableAsync(DateTime.UtcNow)).Select(MapSummary).ToList();

    public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
    {
        ValidateCouponShape(dto.DiscountType, dto.DiscountValue, dto.StartDate, dto.ExpiryDate, dto.MaximumDiscountAmount);

        var normalizedCode = NormalizeCode(dto.CouponCode);
        if (await _couponRepo.GetByCodeAsync(normalizedCode) is not null)
            throw new InvalidOperationException("Coupon code already exists.");

        var coupon = new Coupon
        {
            CouponCode = normalizedCode,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            DiscountType = NormalizeDiscountType(dto.DiscountType),
            DiscountValue = dto.DiscountValue,
            MinimumPurchaseAmount = dto.MinimumPurchaseAmount,
            MaximumDiscountAmount = dto.MaximumDiscountAmount,
            UsageLimit = dto.UsageLimit,
            StartDate = EnsureUtc(dto.StartDate),
            ExpiryDate = dto.ExpiryDate.HasValue ? EnsureUtc(dto.ExpiryDate.Value) : null,
            IsActive = dto.IsActive,
            CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "CAD" : dto.CurrencyCode.Trim().ToUpperInvariant(),
        };

        await _couponRepo.CreateAsync(coupon);
        return Map(coupon);
    }

    public async Task<CouponDto> UpdateAsync(int couponId, UpdateCouponDto dto)
    {
        ValidateCouponShape(dto.DiscountType, dto.DiscountValue, dto.StartDate, dto.ExpiryDate, dto.MaximumDiscountAmount);

        var coupon = await _couponRepo.GetByIdAsync(couponId)
            ?? throw new KeyNotFoundException("Coupon not found.");

        coupon.Title = dto.Title.Trim();
        coupon.Description = dto.Description?.Trim();
        coupon.DiscountType = NormalizeDiscountType(dto.DiscountType);
        coupon.DiscountValue = dto.DiscountValue;
        coupon.MinimumPurchaseAmount = dto.MinimumPurchaseAmount;
        coupon.MaximumDiscountAmount = dto.MaximumDiscountAmount;
        coupon.UsageLimit = dto.UsageLimit;
        coupon.StartDate = EnsureUtc(dto.StartDate);
        coupon.ExpiryDate = dto.ExpiryDate.HasValue ? EnsureUtc(dto.ExpiryDate.Value) : null;
        coupon.IsActive = dto.IsActive;
        coupon.CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? coupon.CurrencyCode : dto.CurrencyCode.Trim().ToUpperInvariant();
        coupon.UpdatedAt = DateTime.UtcNow;

        await _couponRepo.SaveChangesAsync();
        return Map(coupon);
    }

    public async Task<CouponDto> SetActiveAsync(int couponId, bool isActive)
    {
        var coupon = await _couponRepo.GetByIdAsync(couponId)
            ?? throw new KeyNotFoundException("Coupon not found.");

        coupon.IsActive = isActive;
        coupon.UpdatedAt = DateTime.UtcNow;
        await _couponRepo.SaveChangesAsync();
        return Map(coupon);
    }

    public async Task DeleteAsync(int couponId)
    {
        var coupon = await _couponRepo.GetByIdAsync(couponId)
            ?? throw new KeyNotFoundException("Coupon not found.");
        await _couponRepo.DeleteAsync(coupon);
    }

    public async Task<CouponValidationResultDto> ValidateAsync(string couponCode, decimal orderAmount, int? userId = null)
    {
        if (orderAmount <= 0)
            return Invalid(couponCode, orderAmount, "Order amount must be greater than zero.");

        var coupon = await _couponRepo.GetByCodeAsync(couponCode);
        if (coupon is null) return Invalid(couponCode, orderAmount, "Coupon not found.");

        var now = DateTime.UtcNow;
        if (!coupon.IsActive) return Invalid(coupon.CouponCode, orderAmount, "Coupon is inactive.");
        if (coupon.StartDate > now) return Invalid(coupon.CouponCode, orderAmount, "Coupon is not active yet.");
        if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < now)
            return Invalid(coupon.CouponCode, orderAmount, "Coupon has expired.");
        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            return Invalid(coupon.CouponCode, orderAmount, "Coupon usage limit reached.");
        if (coupon.MinimumPurchaseAmount.HasValue && orderAmount < coupon.MinimumPurchaseAmount.Value)
            return Invalid(coupon.CouponCode, orderAmount, $"Minimum purchase amount is {coupon.MinimumPurchaseAmount.Value:0.##}.");
        if (userId.HasValue && await _couponRepo.HasUserUsedCouponAsync(coupon.CouponId, userId.Value))
            return Invalid(coupon.CouponCode, orderAmount, "Coupon already used by this account.");

        var discount = CalculateDiscount(coupon, orderAmount);
        return new CouponValidationResultDto(true, "Coupon applied.", coupon.CouponCode, orderAmount, discount, orderAmount - discount);
    }

    public async Task<IReadOnlyList<CouponUsageStatDto>> GetUsageStatsAsync()
    {
        // Order by the underlying column (c.UsedCount) BEFORE projecting into a
        // record DTO. Sorting on the projected DTO member (s.UsedCount) trips
        // EF Core's translator since the constructor isn't part of the SQL
        // expression tree.
        return await _db.Coupons
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.UsedCount)
            .Select(c => new CouponUsageStatDto(
                c.CouponId,
                c.CouponCode,
                c.Title,
                c.UsedCount,
                c.UsageLimit,
                c.UsageHistory.Sum(h => (decimal?)h.DiscountAmount) ?? 0,
                c.UsageHistory.Sum(h => (decimal?)h.OrderAmount) ?? 0,
                c.UsageHistory.Max(h => (DateTime?)h.UsedAt)))
            .ToListAsync();
    }

    public async Task RecordUsageAsync(string couponCode, int userId, int orderId, decimal orderAmount, decimal discountAmount)
    {
        var coupon = await _couponRepo.GetByCodeAsync(couponCode)
            ?? throw new InvalidOperationException("Coupon not found.");

        coupon.UsedCount += 1;
        coupon.UpdatedAt = DateTime.UtcNow;
        await _couponRepo.RecordUsageAsync(new CouponUsageHistory
        {
            CouponId = coupon.CouponId,
            UserId = userId,
            OrderId = orderId,
            OrderAmount = orderAmount,
            DiscountAmount = discountAmount,
        });
    }

    private static CouponValidationResultDto Invalid(string? couponCode, decimal orderAmount, string message)
        => new(false, message, couponCode?.Trim().ToUpperInvariant(), orderAmount, 0, orderAmount);

    private static decimal CalculateDiscount(Coupon coupon, decimal orderAmount)
    {
        var discount = coupon.DiscountType == CouponDiscountTypes.Percentage
            ? orderAmount * coupon.DiscountValue / 100m
            : coupon.DiscountValue;

        if (coupon.MaximumDiscountAmount.HasValue)
            discount = Math.Min(discount, coupon.MaximumDiscountAmount.Value);

        return Math.Round(Math.Min(discount, orderAmount), 2, MidpointRounding.AwayFromZero);
    }

    private static void ValidateCouponShape(string discountType, decimal discountValue, DateTime start, DateTime? expiry, decimal? maxDiscount)
    {
        var normalizedType = NormalizeDiscountType(discountType);
        if (normalizedType == CouponDiscountTypes.Percentage && discountValue > 100)
            throw new InvalidOperationException("Percentage discount cannot exceed 100.");
        if (maxDiscount.HasValue && maxDiscount.Value <= 0)
            throw new InvalidOperationException("Maximum discount must be greater than zero when provided.");
        if (expiry.HasValue && EnsureUtc(expiry.Value) <= EnsureUtc(start))
            throw new InvalidOperationException("Expiry date must be after start date.");
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static string NormalizeDiscountType(string discountType)
    {
        if (discountType.Equals(CouponDiscountTypes.Percentage, StringComparison.OrdinalIgnoreCase))
            return CouponDiscountTypes.Percentage;
        if (discountType.Equals(CouponDiscountTypes.FixedAmount, StringComparison.OrdinalIgnoreCase))
            return CouponDiscountTypes.FixedAmount;

        throw new InvalidOperationException("Discount type must be FixedAmount or Percentage.");
    }

    private static DateTime EnsureUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static CouponDto Map(Coupon c) => new(
        c.CouponId,
        c.CouponCode,
        c.Title,
        c.Description,
        c.DiscountType,
        c.DiscountValue,
        c.MinimumPurchaseAmount,
        c.MaximumDiscountAmount,
        c.UsageLimit,
        c.UsedCount,
        c.StartDate,
        c.ExpiryDate,
        c.IsActive,
        c.CreatedAt,
        c.UpdatedAt,
        c.CurrencyCode);

    private static CouponSummaryDto MapSummary(Coupon c) => new(
        c.CouponId,
        c.CouponCode,
        c.Title,
        c.Description,
        c.DiscountType,
        c.DiscountValue,
        c.MinimumPurchaseAmount,
        c.MaximumDiscountAmount,
        c.UsageLimit,
        c.UsedCount,
        c.StartDate,
        c.ExpiryDate,
        c.CurrencyCode);
}
