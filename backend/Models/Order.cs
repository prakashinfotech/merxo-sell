namespace MerxoSell.API.Models;

public class Order
{
    public int       OrderId        { get; set; }
    public int       UserId         { get; set; }
    public int?      AddressId      { get; set; }
    public string    Status         { get; set; } = "Pending";
    public decimal   TotalAmountCAD { get; set; }
    public decimal   ShippingAmount { get; set; }
    public decimal   DiscountAmount { get; set; }
    // Snapshot of the display currency and converted total at time of checkout
    public string?   CurrencyCode   { get; set; }
    public decimal?  DisplayTotal   { get; set; }
    public int?      CouponId       { get; set; }
    public string?   CouponCode     { get; set; }
    public string?   Notes          { get; set; }
    public string?   PaymentMethod  { get; set; } = "Razorpay";
    public string?   PaymentTransactionId { get; set; }

    /// <summary>Filled when the order is cancelled — required so admins can audit.</summary>
    public string?   CancellationReason { get; set; }

    /// <summary>UserId of the seller/admin who last approved (transitioned past Pending).</summary>
    public int?      ApprovedBy     { get; set; }

    public DateTime  CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt      { get; set; }

    public User                              User           { get; set; } = null!;
    public Address?                          Address        { get; set; }
    public Coupon?                           Coupon         { get; set; }
    public ICollection<OrderItem>            Items          { get; set; } = [];
    public ICollection<OrderStatusHistory>   StatusHistory  { get; set; } = [];
}
