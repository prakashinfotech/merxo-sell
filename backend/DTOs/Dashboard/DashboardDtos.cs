namespace MerxoSell.API.DTOs.Dashboard;

public record DashboardStatsDto(
    int     TotalProducts,
    int     ActiveProducts,
    int     LowStockProducts,
    int     PendingApprovals,
    int     TotalSellers,
    int     ActiveSellers,
    int     TotalUsers,
    int     TotalCustomers,
    int     ActiveCustomers,
    int     TotalOrders,
    decimal TotalRevenueCAD,
    int     TotalManufacturers,
    int     TotalCategories,
    int     RecentProductAdds,
    int     RecentSales,
    int     RecentCustomerUpdates
);

public record MonthlySalesDto(string Month, decimal Revenue, int OrderCount);
public record DailyViewDto(string Date, int Views);
public record ChartDataDto<T>(IEnumerable<T> Data, string Label);

public record TopProductDto(
    int      ProductId,
    string   Name,
    string?  ImageUrl,
    int      UnitsSold,
    decimal  Revenue
);

public record CategoryBreakdownDto(
    string   Category,
    int      OrderCount,
    decimal  Revenue,
    double   Percentage
);

public record DashboardRecentOrderDto(
    int      OrderId,
    string   Status,
    decimal  TotalAmountCAD,
    string   CurrencyCode,
    decimal  DisplayTotal,
    string   BuyerName,
    DateTime CreatedAt
);
