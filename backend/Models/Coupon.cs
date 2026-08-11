using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.Models;

public class Coupon
{
    public int CouponId { get; set; }

    [Required, MaxLength(50)]
    public string CouponCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(30)]
    public string DiscountType { get; set; } = CouponDiscountTypes.FixedAmount;

    public decimal DiscountValue { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    /// <summary>Currency for FixedAmount discounts (ISO code, e.g. CAD, USD). Percentage coupons apply universally.</summary>
    [MaxLength(10)]
    public string CurrencyCode { get; set; } = "CAD";

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<CouponUsageHistory> UsageHistory { get; set; } = [];
}

public static class CouponDiscountTypes
{
    public const string FixedAmount = "FixedAmount";
    public const string Percentage = "Percentage";
}
