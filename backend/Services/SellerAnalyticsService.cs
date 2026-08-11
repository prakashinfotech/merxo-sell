using MerxoSell.API.DTOs.Seller;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class SellerAnalyticsService : ISellerAnalyticsService
{
    private readonly ISellerAnalyticsRepository _repo;
    public SellerAnalyticsService(ISellerAnalyticsRepository repo) => _repo = repo;

    public Task<SellerAnalyticsSummaryDto> GetSummaryAsync(int sellerId, DateTime fromUtc, DateTime toUtc)
        => _repo.GetSummaryAsync(sellerId, fromUtc, toUtc);
}
