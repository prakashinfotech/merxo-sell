namespace MerxoSell.API.DTOs.Seller;

/// <summary>One day of revenue. Used by the seller-dashboard line chart.</summary>
public record RevenueByDayDto(
    DateTime Date,
    decimal  RevenueCAD,
    int      OrderCount
);

/// <summary>Top-product row for the seller-dashboard bar chart.</summary>
public record AnalyticsTopProductDto(
    int      ProductId,
    string   Name,
    string?  PrimaryImageUrl,
    int      UnitsSold,
    decimal  RevenueCAD
);

/// <summary>Aggregate KPIs for the past 30 days plus the chart series.</summary>
public record SellerAnalyticsSummaryDto(
    decimal                    TotalSalesCAD,        // last 30 days
    int                        OrderCount,           // last 30 days, distinct orders
    int                        UnitsSold,            // last 30 days
    decimal                    AverageOrderValue,    // TotalSales / OrderCount
    decimal                    ConversionRate,       // distinct buyers / unique product viewers
    int                        TotalViews,
    int                        UniqueViewers,
    IReadOnlyList<AnalyticsTopProductDto> TopProducts,
    IReadOnlyList<RevenueByDayDto>        RevenueByDay
);
