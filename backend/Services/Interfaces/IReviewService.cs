using MerxoSell.API.DTOs.Reviews;

namespace MerxoSell.API.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> AddReviewAsync(int userId, CreateReviewDto dto);
    Task<IEnumerable<MyReviewDto>> GetMyReviewsAsync(int userId);
    Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId);
    Task<IEnumerable<PortalReviewDto>> GetPortalReviewsAsync(string? search, int? rating);
    Task<IEnumerable<PortalReviewDto>> GetSellerReviewsAsync(int sellerId, string? search, int? rating);
}
