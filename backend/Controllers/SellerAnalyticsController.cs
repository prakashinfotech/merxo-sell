using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MerxoSell.API.DTOs.Seller;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>
/// Seller analytics. Sellers can only see their own data — sellerId is taken
/// from the JWT, never from the URL.
/// </summary>
[Route("api/seller/analytics")]
[Authorize(Policy = "Seller")]
public class SellerAnalyticsController : BaseApiController
{
    private readonly ISellerAnalyticsService _analytics;

    public SellerAnalyticsController(ISellerAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    private int GetSellerId()
    {
        var claim = User.FindFirstValue("sellerId");
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var id))
            throw new UnauthorizedAccessException("Seller identity not found in token.");
        return id;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SellerAnalyticsSummaryDto>> GetSummary(
        [FromQuery] string?   range = "30d",
        [FromQuery] DateTime? from  = null,
        [FromQuery] DateTime? to    = null)
    {
        var sellerId = GetSellerId();
        var (fromUtc, toUtc) = ResolveWindow(range, from, to);
        var summary = await _analytics.GetSummaryAsync(sellerId, fromUtc, toUtc);
        return Ok(summary);
    }

    private static (DateTime fromUtc, DateTime toUtc) ResolveWindow(string? range, DateTime? from, DateTime? to)
    {
        var nowUtc   = DateTime.UtcNow;
        var todayEnd = nowUtc.Date.AddDays(1).AddTicks(-1);
        switch ((range ?? "30d").Trim().ToLowerInvariant())
        {
            case "7d": return (nowUtc.Date.AddDays(-6), todayEnd);
            case "month":
            case "last-month":
            {
                var first = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                return (first.AddMonths(-1), first.AddTicks(-1));
            }
            case "this-month":
            {
                var first = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                return (first, todayEnd);
            }
            case "custom":
                if (from.HasValue && to.HasValue)
                {
                    var f = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);
                    var t = DateTime.SpecifyKind(to.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                    return (f, t);
                }
                goto default;
            default: return (nowUtc.Date.AddDays(-29), todayEnd);
        }
    }
}
