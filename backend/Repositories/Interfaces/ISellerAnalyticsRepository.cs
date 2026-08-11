using MerxoSell.API.DTOs.Seller;

namespace MerxoSell.API.Repositories.Interfaces;

public interface ISellerAnalyticsRepository
{
    Task<SellerAnalyticsSummaryDto> GetSummaryAsync(int sellerId, DateTime fromUtc, DateTime toUtc);
}
