using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Admin;

/// <summary>Lightweight order row for the admin orders list table.</summary>
public record AdminOrderListDto(
    int      OrderId,
    string   Status,
    decimal  TotalAmountCAD,
    string   CurrencyCode,
    decimal  DisplayTotal,
    int      ItemCount,
    int      UserId,
    string   BuyerName,
    string   BuyerEmail,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string?  PaymentMethod = null,
    string?  PaymentTransactionId = null
);

public record AdminOrderItemDto(
    int      OrderItemId,
    int      ProductId,
    int?     VariantId,
    string   ProductName,
    string?  PrimaryImageUrl,
    string?  VariantInfo,
    int      Quantity,
    decimal  UnitPriceCAD
);

public record AdminOrderShippingAddressDto(
    string  FullName,
    string  Phone,
    string  AddressLine1,
    string? AddressLine2,
    string  City,
    string? State,
    string  PostalCode,
    string  Country
);

/// <summary>Full order detail for the admin view modal.</summary>
public record AdminOrderDetailDto(
    int      OrderId,
    string   Status,
    decimal  TotalAmountCAD,
    decimal  ShippingAmount,
    decimal  DiscountAmount,
    decimal  DisplayTotal,
    string   CurrencyCode,
    string?  Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int      UserId,
    string   BuyerName,
    string   BuyerEmail,
    string?  BuyerPhone,
    AdminOrderShippingAddressDto? ShippingAddress,
    IEnumerable<AdminOrderItemDto> Items,
    string?  PaymentMethod = null,
    string?  PaymentTransactionId = null
);

public record UpdateOrderStatusDto(
    [Required, MaxLength(30)] string Status,
    [MaxLength(500)] string? Note
);
