using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Orders;

public record OrderItemDto(
    int      OrderItemId,
    int      ProductId,
    int?     VariantId,
    string   ProductName,
    string?  PrimaryImageUrl,
    string?  VariantInfo,
    int      Quantity,
    decimal  UnitPriceCAD
);

public record OrderDto(
    int      OrderId,
    string   Status,
    decimal  TotalAmountCAD,
    decimal  ShippingAmount,
    decimal  DiscountAmount,
    string?  CouponCode,
    decimal  DisplayTotal,
    string   CurrencyCode,
    DateTime CreatedAt,
    string?  CancellationReason,
    IEnumerable<OrderItemDto> Items
);

public record CreateOrderDto(
    [Required] int AddressId,
    [Required] string CurrencyCode,
    [Required] decimal DisplayTotal,
    decimal ShippingAmount,
    decimal DiscountAmount,
    [MaxLength(50)] string? CouponCode,
    IList<CreateOrderItemDto>? Items = null
);

public record CreateOrderItemDto(
    [Required] int ProductId,
    int?          VariantId,
    [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
                  int Quantity
);

public record UpdateOrderStatusDto(
    [Required, MaxLength(30)] string Status,
    [MaxLength(500)]          string? Note
);

public record CancelOrderDto(
    [Required, MaxLength(1000)] string CancellationReason
);

public record OrderStatusHistoryDto(
    int      OrderStatusHistoryId,
    int      OrderId,
    string   FromStatus,
    string   ToStatus,
    string?  Note,
    int?     ChangedBy,
    string?  ChangedByName,
    string?  ChangedByRole,
    DateTime CreatedAt
);
