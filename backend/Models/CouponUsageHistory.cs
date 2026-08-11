namespace MerxoSell.API.Models;

public class CouponUsageHistory
{
    public int CouponUsageHistoryId { get; set; }
    public int CouponId { get; set; }
    public int UserId { get; set; }
    public int? OrderId { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    public Coupon Coupon { get; set; } = null!;
    public User User { get; set; } = null!;
    public Order? Order { get; set; }
}
