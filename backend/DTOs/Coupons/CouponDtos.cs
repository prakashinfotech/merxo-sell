using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Coupons;

public record CouponDto(
    int CouponId,
    string CouponCode,
    string Title,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinimumPurchaseAmount,
    decimal? MaximumDiscountAmount,
    int? UsageLimit,
    int UsedCount,
    DateTime StartDate,
    DateTime? ExpiryDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CurrencyCode
);

public record CouponSummaryDto(
    int CouponId,
    string CouponCode,
    string Title,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinimumPurchaseAmount,
    decimal? MaximumDiscountAmount,
    int? UsageLimit,
    int UsedCount,
    DateTime StartDate,
    DateTime? ExpiryDate,
    string CurrencyCode
);

public record CreateCouponDto(
    [Required, MaxLength(50)] string CouponCode,
    [Required, MaxLength(150)] string Title,
    [MaxLength(1000)] string? Description,
    [Required, MaxLength(30)] string DiscountType,
    [Range(0.01, 999999999)] decimal DiscountValue,
    [Range(0, 999999999)] decimal? MinimumPurchaseAmount,
    [Range(0, 999999999)] decimal? MaximumDiscountAmount,
    [Range(1, int.MaxValue)] int? UsageLimit,
    DateTime StartDate,
    DateTime? ExpiryDate,
    bool IsActive = true,
    [MaxLength(10)] string CurrencyCode = "CAD"
);

public record UpdateCouponDto(
    [Required, MaxLength(150)] string Title,
    [MaxLength(1000)] string? Description,
    [Required, MaxLength(30)] string DiscountType,
    [Range(0.01, 999999999)] decimal DiscountValue,
    [Range(0, 999999999)] decimal? MinimumPurchaseAmount,
    [Range(0, 999999999)] decimal? MaximumDiscountAmount,
    [Range(1, int.MaxValue)] int? UsageLimit,
    DateTime StartDate,
    DateTime? ExpiryDate,
    bool IsActive,
    [MaxLength(10)] string CurrencyCode = "CAD"
);

public record ApplyCouponDto(
    [Required, MaxLength(50)] string CouponCode,
    [Range(0.01, 999999999)] decimal OrderAmount
);

public record CouponValidationResultDto(
    bool IsValid,
    string? Message,
    string? CouponCode,
    decimal OrderAmount,
    decimal DiscountAmount,
    decimal FinalAmount
);

public record CouponUsageStatDto(
    int CouponId,
    string CouponCode,
    string Title,
    int UsedCount,
    int? UsageLimit,
    decimal TotalDiscountGiven,
    decimal TotalOrderAmount,
    DateTime? LastUsedAt
);
