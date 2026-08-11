using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IReviewRepository
{
    Task<Review> CreateReviewAsync(Review review);
    Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
    Task<IEnumerable<Review>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Review>> GetAllForPortalAsync(string? search, int? rating);
    Task<IEnumerable<Review>> GetBySellerAsync(int sellerId, string? search, int? rating);
    Task<bool> HasUserPurchasedProductAsync(int userId, int productId);
    Task<bool> HasUserReviewedProductAsync(int userId, int productId);

    /// <summary>Reviews currently flagged for moderator attention.</summary>
    Task<IEnumerable<Review>> GetFlaggedAsync();

    /// <summary>Single review with product + author included for the moderator UI.</summary>
    Task<Review?> GetByIdForModerationAsync(int reviewId);

    /// <summary>Updates the moderation status, audit columns, and persists.</summary>
    Task UpdateModerationStatusAsync(Review review);
}
