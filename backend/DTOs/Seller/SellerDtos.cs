using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Seller;

public record SellerDto(
    int     SellerId,
    int     UserId,
    string  StoreName,
    string? StoreDescription,
    string? ContactEmail,
    string? Phone,
    bool    IsVerified,
    bool    IsActive,
    string  OwnerEmail,
    string  OwnerName,
    int     ProductCount,
    DateTime CreatedAt
);

public record CreateSellerDto(
    [Required, MaxLength(200)] string  StoreName,
    [MaxLength(1000)]          string? StoreDescription,
    [EmailAddress, MaxLength(200)] string? ContactEmail,
    [MaxLength(50)] string? Phone,
    [Required] int UserId
);

public record UpdateSellerDto(
    [Required, MaxLength(200)] string  StoreName,
    [MaxLength(1000)]          string? StoreDescription,
    [EmailAddress, MaxLength(200)] string? ContactEmail,
    [MaxLength(50)] string? Phone
);

public record SetSellerStatusDto([Required] bool IsActive);

public record SetSellerVerifiedDto([Required] bool IsVerified);

public record SellerDashboardDto(
    int     TotalProducts,
    int     ActiveProducts,
    int     LowStockProducts,
    int     TotalOrders,
    decimal TotalRevenueCAD,
    int     RecentProductAdds,
    int     RecentSales,
    IEnumerable<TopProductDto>? TopProducts = null
);

public record TopProductDto(
    int     ProductId,
    string  Name,
    string? ImageUrl,
    int     ThisMonthViews,
    int     LastMonthViews,
    double  PercentageChange
);
