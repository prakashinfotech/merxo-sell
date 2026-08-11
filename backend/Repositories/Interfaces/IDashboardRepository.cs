using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardStatsData> GetStatsAsync(DateTime fromUtc, DateTime toUtc);
    Task<IEnumerable<DailyRevenueData>> GetDailyRevenueAsync(DateTime fromUtc, DateTime toUtc);
    Task<IEnumerable<DailyViewData>> GetDailyViewsAsync(DateTime fromUtc, DateTime toUtc);
    Task<IEnumerable<MonthlySalesData>> GetMonthlySalesAsync(int months);
    Task<IEnumerable<TopProductData>> GetTopProductsAsync(DateTime fromUtc, DateTime toUtc, int limit = 5);
    Task<IEnumerable<CategoryBreakdownData>> GetCategoryBreakdownAsync(DateTime fromUtc, DateTime toUtc);
    Task<IEnumerable<RecentOrderData>> GetRecentOrdersAsync(int limit = 5);
}

public record DashboardStatsData(
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

public record MonthlySalesData(string Month, decimal Revenue, int OrderCount);
public record DailyViewData(string Date, int Views);
public record DailyRevenueData(string Date, decimal Revenue, int OrderCount);
public record TopProductData(int ProductId, string Name, string? ImageUrl, int UnitsSold, decimal Revenue);
public record CategoryBreakdownData(string Category, int OrderCount, decimal Revenue, double Percentage);
public record RecentOrderData(int OrderId, string Status, decimal TotalAmountCAD, string CurrencyCode, decimal DisplayTotal, string BuyerName, DateTime CreatedAt);
