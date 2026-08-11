using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Dashboard;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>
/// Analytics dashboard for the Admin Portal. Every endpoint accepts the same
/// `range` query (`30d` | `7d` | `month` | `custom`) plus optional `from`/`to`
/// ISO dates so the chart and KPI cards stay in sync.
/// </summary>
[Route("api/dashboard")]
[Authorize(Roles = "SuperAdmin")]
public class DashboardController : BaseApiController
{
    private readonly IDashboardRepository _dashboardRepo;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardRepository dashboardRepo, ILogger<DashboardController> logger)
    {
        _dashboardRepo = dashboardRepo;
        _logger        = logger;
    }

    /// <summary>GET /api/dashboard/stats — KPI block. Supports range filter.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), 200)]
    public async Task<IActionResult> GetStats(
        [FromQuery] string? range = "30d",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null)
    {
        try
        {
            var (fromUtc, toUtc) = ResolveWindow(range, from, to);
            var data = await _dashboardRepo.GetStatsAsync(fromUtc, toUtc);
            return Ok(new DashboardStatsDto(
                data.TotalProducts, data.ActiveProducts, data.LowStockProducts,
                data.PendingApprovals, data.TotalSellers, data.ActiveSellers,
                data.TotalUsers, data.TotalCustomers, data.ActiveCustomers,
                data.TotalOrders, data.TotalRevenueCAD, data.TotalManufacturers,
                data.TotalCategories, data.RecentProductAdds, data.RecentSales, data.RecentCustomerUpdates));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics");
            return StatusCode(500, new { error = "An internal error occurred while fetching statistics." });
        }
    }

    /// <summary>GET /api/dashboard/revenue — daily revenue series for the chart.</summary>
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] string? range = "30d",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null)
    {
        try
        {
            var (fromUtc, toUtc) = ResolveWindow(range, from, to);
            var data = await _dashboardRepo.GetDailyRevenueAsync(fromUtc, toUtc);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching daily revenue");
            return StatusCode(500, new { error = "An internal error occurred while fetching revenue data." });
        }
    }

    /// <summary>GET /api/dashboard/monthly-sales?months=6 — legacy chart data.</summary>
    [HttpGet("monthly-sales")]
    [ProducesResponseType(typeof(IEnumerable<MonthlySalesDto>), 200)]
    public async Task<IActionResult> GetMonthlySales([FromQuery] int months = 6)
    {
        try
        {
            if (months <= 0 || months > 24) months = 6;
            var data = await _dashboardRepo.GetMonthlySalesAsync(months);
            var dtos = data.Select(d => new MonthlySalesDto(d.Month, d.Revenue, d.OrderCount));
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching monthly sales data");
            return StatusCode(500, new { error = "An internal error occurred while fetching sales data." });
        }
    }

    /// <summary>GET /api/dashboard/daily-views — supports the same range filter as stats.</summary>
    [HttpGet("daily-views")]
    [ProducesResponseType(typeof(IEnumerable<DailyViewDto>), 200)]
    public async Task<IActionResult> GetDailyViews(
        [FromQuery] string? range = "30d",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null)
    {
        try
        {
            var (fromUtc, toUtc) = ResolveWindow(range, from, to);
            var data = await _dashboardRepo.GetDailyViewsAsync(fromUtc, toUtc);
            var dtos = data.Select(d => new DailyViewDto(d.Date, d.Views));
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching daily views data");
            return StatusCode(500, new { error = "An internal error occurred while fetching traffic data." });
        }
    }

    /// <summary>GET /api/dashboard/top-products — top N products by revenue in the window.</summary>
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] string? range = "30d",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null,
        [FromQuery] int limit = 5)
    {
        try
        {
            if (limit <= 0 || limit > 20) limit = 5;
            var (fromUtc, toUtc) = ResolveWindow(range, from, to);
            var data = await _dashboardRepo.GetTopProductsAsync(fromUtc, toUtc, limit);
            return Ok(data.Select(d => new TopProductDto(d.ProductId, d.Name, d.ImageUrl, d.UnitsSold, d.Revenue)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top products");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>GET /api/dashboard/category-breakdown — revenue share by top categories.</summary>
    [HttpGet("category-breakdown")]
    public async Task<IActionResult> GetCategoryBreakdown(
        [FromQuery] string? range = "30d",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null)
    {
        try
        {
            var (fromUtc, toUtc) = ResolveWindow(range, from, to);
            var data = await _dashboardRepo.GetCategoryBreakdownAsync(fromUtc, toUtc);
            return Ok(data.Select(d => new CategoryBreakdownDto(d.Category, d.OrderCount, d.Revenue, d.Percentage)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category breakdown");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>GET /api/dashboard/recent-orders — latest N orders for the dashboard table.</summary>
    [HttpGet("recent-orders")]
    public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 5)
    {
        try
        {
            if (limit <= 0 || limit > 20) limit = 5;
            var data = await _dashboardRepo.GetRecentOrdersAsync(limit);
            return Ok(data.Select(d => new DashboardRecentOrderDto(
                d.OrderId, d.Status, d.TotalAmountCAD, d.CurrencyCode, d.DisplayTotal, d.BuyerName, d.CreatedAt)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent orders");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>
    /// Resolve the supplied filter arguments into an inclusive [fromUtc, toUtc]
    /// window. Defaults to the last 30 calendar days. `custom` requires both
    /// `from` and `to` — otherwise we fall back to the 30-day default.
    /// </summary>
    private static (DateTime fromUtc, DateTime toUtc) ResolveWindow(string? range, DateTime? from, DateTime? to)
    {
        var nowUtc = DateTime.UtcNow;
        var todayEnd = nowUtc.Date.AddDays(1).AddTicks(-1);

        switch ((range ?? "30d").Trim().ToLowerInvariant())
        {
            case "7d":
            case "week":
            case "last-week":
                return (nowUtc.Date.AddDays(-6), todayEnd);

            case "month":
            case "last-month":
            {
                var firstOfThisMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var firstOfLastMonth = firstOfThisMonth.AddMonths(-1);
                var endOfLastMonth   = firstOfThisMonth.AddTicks(-1);
                return (firstOfLastMonth, endOfLastMonth);
            }

            case "this-month":
            {
                var firstOfThisMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                return (firstOfThisMonth, todayEnd);
            }

            case "custom":
                if (from.HasValue && to.HasValue)
                {
                    var f = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);
                    var t = DateTime.SpecifyKind(to.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                    return (f, t);
                }
                goto default;

            default: // "30d"
                return (nowUtc.Date.AddDays(-29), todayEnd);
        }
    }
}
