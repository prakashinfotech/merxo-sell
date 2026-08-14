using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Constants;
using MerxoSell.API.Data;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _db;
    public DashboardRepository(AppDbContext db) => _db = db;

    public async Task<DashboardStatsData> GetStatsAsync(DateTime fromUtc, DateTime toUtc)
    {
        // ── Catalog totals are not date-bound ──────────────────────────
        var totalProducts    = await _db.Products.CountAsync();
        var activeProducts   = await _db.Products.CountAsync(p => p.IsActive);
        var lowStock         = await _db.Products.CountAsync(p => p.Stock <= 10 && p.IsActive);
        var pendingApprovals = await _db.Products.CountAsync(p => p.Status == "Pending" && p.IsActive && !p.IsDeleted);
        var totalSellers     = await _db.Sellers.CountAsync();
        var activeSellers    = await _db.Sellers.CountAsync(s => s.IsActive);
        var totalUsers       = await _db.Users.CountAsync(u => u.IsActive);
        var totalCustomers   = await _db.Users.CountAsync(u => u.Role.RoleName == AppRoles.Buyer);
        var activeCustomers  = await _db.Users.CountAsync(u => u.Role.RoleName == AppRoles.Buyer && u.IsActive);
        var totalMfr         = await _db.Manufacturers.CountAsync(m => m.IsActive);
        var totalCategories  = await _db.Categories.CountAsync();

        // ── Date-bound metrics ─────────────────────────────────────────
        var ordersInWindow = _db.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc);

        var totalOrders     = await ordersInWindow.CountAsync();
        var totalRevenue    = await ordersInWindow.SumAsync(o => (decimal?)o.TotalAmountCAD) ?? 0m;
        var recentProducts  = await _db.Products.CountAsync(p => p.CreatedAt >= fromUtc && p.CreatedAt <= toUtc);
        var recentSales     = totalOrders;
        var recentUserUpdates = await _db.Users.CountAsync(u => u.UpdatedAt >= fromUtc && u.UpdatedAt <= toUtc);

        return new DashboardStatsData(
            totalProducts, activeProducts, lowStock,
            pendingApprovals, totalSellers, activeSellers,
            totalUsers, totalCustomers, activeCustomers,
            totalOrders, totalRevenue, totalMfr,
            totalCategories, recentProducts, recentSales, recentUserUpdates);
    }

    public async Task<IEnumerable<MonthlySalesData>> GetMonthlySalesAsync(int months)
    {
        var from = DateTime.UtcNow.AddMonths(-months + 1);
        var data = await _db.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= from)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue    = g.Sum(o => o.TotalAmountCAD),
                OrderCount = g.Count()
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        return data.Select(d => new MonthlySalesData(
            $"{d.Year}-{d.Month:D2}",
            d.Revenue,
            d.OrderCount));
    }

    public async Task<IEnumerable<DailyRevenueData>> GetDailyRevenueAsync(DateTime fromUtc, DateTime toUtc)
    {
        var rows = await _db.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date    = g.Key,
                Revenue = g.Sum(o => o.TotalAmountCAD),
                Orders  = g.Count()
            })
            .ToListAsync();

        var byDay = rows.ToDictionary(r => r.Date, r => r);
        var totalDays = (int)Math.Max(1, (toUtc.Date - fromUtc.Date).TotalDays + 1);
        return Enumerable.Range(0, totalDays)
            .Select(offset => fromUtc.Date.AddDays(offset))
            .Select(d => byDay.TryGetValue(d, out var row)
                ? new DailyRevenueData(d.ToString("yyyy-MM-dd"), row.Revenue, row.Orders)
                : new DailyRevenueData(d.ToString("yyyy-MM-dd"), 0m, 0))
            .ToList();
    }

    public async Task<IEnumerable<DailyViewData>> GetDailyViewsAsync(DateTime fromUtc, DateTime toUtc)
    {
        var rows = await _db.ProductViews
            .AsNoTracking()
            .Where(v => v.ViewedAt >= fromUtc && v.ViewedAt <= toUtc)
            .GroupBy(v => v.ViewedAt.Date)
            .Select(g => new { Date = g.Key, Views = g.Count() })
            .ToListAsync();

        var byDay = rows.ToDictionary(r => r.Date, r => r.Views);
        var totalDays = (int)Math.Max(1, (toUtc.Date - fromUtc.Date).TotalDays + 1);
        return Enumerable.Range(0, totalDays)
            .Select(offset => fromUtc.Date.AddDays(offset))
            .Select(d => new DailyViewData(d.ToString("yyyy-MM-dd"),
                byDay.TryGetValue(d, out var v) ? v : 0))
            .ToList();
    }

    public async Task<IEnumerable<TopProductData>> GetTopProductsAsync(DateTime fromUtc, DateTime toUtc, int limit = 5)
    {
        var groups = await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.Order.CreatedAt >= fromUtc && i.Order.CreatedAt <= toUtc
                        && i.Order.Status != "Cancelled")
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                UnitsSold = g.Sum(i => i.Quantity),
                Revenue   = g.Sum(i => i.UnitPriceCAD * i.Quantity),
            })
            .OrderByDescending(g => g.Revenue)
            .Take(limit)
            .ToListAsync();

        var productIds = groups.Select(g => g.ProductId).ToList();
        var images = productIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.ProductImages
                .AsNoTracking()
                .Where(img => productIds.Contains(img.ProductId))
                .OrderByDescending(img => img.IsPrimary)
                .GroupBy(img => img.ProductId)
                .Select(g => new { ProductId = g.Key, ImageUrl = g.First().ImageUrl })
                .ToDictionaryAsync(img => img.ProductId, img => img.ImageUrl);

        return groups.Select(g => new TopProductData(
            g.ProductId, g.ProductName,
            images.TryGetValue(g.ProductId, out var url) ? url : null,
            g.UnitsSold, g.Revenue));
    }

    public async Task<IEnumerable<CategoryBreakdownData>> GetCategoryBreakdownAsync(DateTime fromUtc, DateTime toUtc)
    {
        var rows = await (
            from oi in _db.OrderItems
            join o  in _db.Orders     on oi.OrderId   equals o.OrderId
            join p  in _db.Products   on oi.ProductId equals p.ProductId
            join c  in _db.Categories on p.CategoryId equals c.CategoryId
            where o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc
                  && o.Status != "Cancelled"
            group new { oi.UnitPriceCAD, oi.Quantity } by c.Name into g
            select new
            {
                Category   = g.Key,
                OrderCount = g.Count(),
                Revenue    = g.Sum(x => x.UnitPriceCAD * x.Quantity),
            }
        ).OrderByDescending(g => g.Revenue).Take(6).ToListAsync();

        var total = rows.Sum(r => r.Revenue);
        return rows.Select(r => new CategoryBreakdownData(
            r.Category ?? "Other", r.OrderCount, r.Revenue,
            total > 0 ? Math.Round((double)(r.Revenue / total) * 100, 1) : 0));
    }

    public async Task<IEnumerable<RecentOrderData>> GetRecentOrdersAsync(int limit = 5)
    {
        return await _db.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .Select(o => new RecentOrderData(
                o.OrderId, o.Status, o.TotalAmountCAD,
                o.CurrencyCode ?? "CAD",
                o.DisplayTotal ?? o.TotalAmountCAD,
                o.User.FullName, o.CreatedAt))
            .ToListAsync();
    }
}
