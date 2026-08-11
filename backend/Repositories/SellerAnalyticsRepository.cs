using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Seller;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class SellerAnalyticsRepository : ISellerAnalyticsRepository
{
    private readonly AppDbContext _db;
    public SellerAnalyticsRepository(AppDbContext db) => _db = db;

    public async Task<SellerAnalyticsSummaryDto> GetSummaryAsync(int sellerId, DateTime fromUtc, DateTime toUtc)
    {
        // ── Sold items in the window (cancelled orders excluded) ────────
        var sellerItems = _db.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Product.SellerId == sellerId
                      && oi.Order.CreatedAt >= fromUtc
                      && oi.Order.CreatedAt <= toUtc
                      && oi.Order.Status != "Cancelled");

        var totalSalesCAD = await sellerItems.SumAsync(oi => (decimal?)oi.Quantity * oi.UnitPriceCAD) ?? 0m;
        var unitsSold     = await sellerItems.SumAsync(oi => (int?)oi.Quantity) ?? 0;
        var orderCount    = await sellerItems.Select(oi => oi.OrderId).Distinct().CountAsync();

        // ── Daily revenue series (every day in the window) ──────────────
        var dailyRows = await sellerItems
            .GroupBy(oi => oi.Order.CreatedAt.Date)
            .Select(g => new
            {
                Date    = g.Key,
                Revenue = g.Sum(oi => oi.Quantity * oi.UnitPriceCAD),
                Orders  = g.Select(x => x.OrderId).Distinct().Count()
            })
            .ToListAsync();

        var byDayMap   = dailyRows.ToDictionary(r => r.Date, r => r);
        var fromDate   = fromUtc.Date;
        var toDate     = toUtc.Date;
        var dayCount   = (toDate - fromDate).Days + 1;

        var revenueByDay = Enumerable.Range(0, dayCount)
            .Select(offset => fromDate.AddDays(offset))
            .Select(d => byDayMap.TryGetValue(d, out var row)
                ? new RevenueByDayDto(d, row.Revenue, row.Orders)
                : new RevenueByDayDto(d, 0m, 0))
            .ToList();

        // ── Top 5 products by revenue in the window ─────────────────────
        var topRows = await sellerItems
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.Name,
                Units   = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.UnitPriceCAD)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync();

        var topProductIds = topRows.Select(t => t.ProductId).ToList();
        var primaryImages = await _db.ProductImages
            .AsNoTracking()
            .Where(i => topProductIds.Contains(i.ProductId))
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Url       = g.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.ProductId, x => x.Url);

        var topProducts = topRows.Select(r => new AnalyticsTopProductDto(
            r.ProductId, r.Name,
            primaryImages.TryGetValue(r.ProductId, out var url) ? url : null,
            r.Units, r.Revenue)).ToList();

        // ── Conversion rate ─────────────────────────────────────────────
        var totalViews = await _db.ProductViews
            .AsNoTracking()
            .CountAsync(v => v.Product.SellerId == sellerId
                          && v.ViewedAt >= fromUtc && v.ViewedAt <= toUtc);

        var uniqueViewers = await _db.ProductViews
            .AsNoTracking()
            .Where(v => v.Product.SellerId == sellerId
                     && v.ViewedAt >= fromUtc && v.ViewedAt <= toUtc
                     && v.UserId != null)
            .Select(v => v.UserId)
            .Distinct()
            .CountAsync();

        var distinctBuyers = await sellerItems
            .Select(oi => oi.Order.UserId)
            .Distinct()
            .CountAsync();

        var conversionRate = uniqueViewers > 0
            ? Math.Round((decimal)distinctBuyers / uniqueViewers * 100m, 2)
            : 0m;

        var avgOrderValue = orderCount > 0 ? Math.Round(totalSalesCAD / orderCount, 2) : 0m;

        return new SellerAnalyticsSummaryDto(
            totalSalesCAD, orderCount, unitsSold, avgOrderValue,
            conversionRate, totalViews, uniqueViewers,
            topProducts, revenueByDay);
    }
}
