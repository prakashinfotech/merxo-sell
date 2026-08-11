namespace MerxoSell.API.DTOs.Cart;

public record CartItemDto(
    int      CartItemId,
    int      ProductId,
    int?     VariantId,
    string   ProductName,
    string?  PrimaryImageUrl,
    string?  VariantInfo, // e.g. "Color: Red, Size: M"
    decimal  UnitPriceCAD,
    int      Quantity,
    int      MaxStock
);

public record CartDto(
    int     CartId,
    int     UserId,
    decimal TotalAmountCAD,
    IEnumerable<CartItemDto> Items
);

public record AddToCartDto(
    int ProductId,
    int? VariantId,
    int Quantity
);

public record UpdateCartItemDto(
    int Quantity
);
