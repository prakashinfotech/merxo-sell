using MerxoSell.API.DTOs.Seller;

namespace MerxoSell.API.Services.Interfaces;

public interface ISellerAnalyticsService
{
    Task<SellerAnalyticsSummaryDto> GetSummaryAsync(int sellerId, DateTime fromUtc, DateTime toUtc);
}
