using MerxoSell.API.DTOs.Reviews;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;

    public ReviewService(IReviewRepository reviewRepo)
    {
        _reviewRepo = reviewRepo;
    }

    public async Task<ReviewDto> AddReviewAsync(int userId, CreateReviewDto dto)
    {
        if (await _reviewRepo.HasUserReviewedProductAsync(userId, dto.ProductId))
            throw new InvalidOperationException("You have already reviewed this product.");

        if (!await _reviewRepo.HasUserPurchasedProductAsync(userId, dto.ProductId))
            throw new InvalidOperationException("You must purchase the product before reviewing it.");

        var review = new Review
        {
            UserId = userId,
            ProductId = dto.ProductId,
            Rating = (byte)dto.Rating,
            Comment = dto.Comment
        };

        var created = await _reviewRepo.CreateReviewAsync(review);
        
        // Reload to get User info
        var reviews = await _reviewRepo.GetByProductIdAsync(dto.ProductId);
        var createdWithUser = reviews.First(r => r.ReviewId == created.ReviewId);

        return new ReviewDto(
            createdWithUser.ReviewId,
            createdWithUser.ProductId,
            createdWithUser.UserId,
            createdWithUser.User.FullName,
            createdWithUser.Rating,
            createdWithUser.Comment,
            createdWithUser.CreatedAt
        );
    }

    public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId)
    {
        var reviews = await _reviewRepo.GetByProductIdAsync(productId);
        return reviews.Select(r => new ReviewDto(
            r.ReviewId,
            r.ProductId,
            r.UserId,
            r.User?.FullName ?? "Anonymous",
            r.Rating,
            r.Comment,
            r.CreatedAt
        ));
    }

    public async Task<IEnumerable<MyReviewDto>> GetMyReviewsAsync(int userId)
    {
        var reviews = await _reviewRepo.GetByUserIdAsync(userId);
        return reviews.Select(r => new MyReviewDto(
            r.ReviewId,
            r.ProductId,
            r.Product?.Name ?? "Unknown product",
            r.Product?.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ?? r.Product?.Images.FirstOrDefault()?.ImageUrl,
            r.Rating,
            r.Comment,
            r.CreatedAt
        ));
    }

    public async Task<IEnumerable<PortalReviewDto>> GetPortalReviewsAsync(string? search, int? rating)
    {
        var reviews = await _reviewRepo.GetAllForPortalAsync(search, rating);
        return reviews.Select(ToPortalDto);
    }

    public async Task<IEnumerable<PortalReviewDto>> GetSellerReviewsAsync(int sellerId, string? search, int? rating)
    {
        var reviews = await _reviewRepo.GetBySellerAsync(sellerId, search, rating);
        return reviews.Select(ToPortalDto);
    }

    private static PortalReviewDto ToPortalDto(Review r) =>
        new(
            r.ReviewId,
            r.ProductId,
            r.Product?.Name ?? "Unknown product",
            r.UserId,
            r.User?.FullName ?? "Anonymous",
            r.User?.Email ?? string.Empty,
            r.Rating,
            r.Comment,
            r.CreatedAt,
            r.Product?.SellerId,
            r.Product?.Seller?.StoreName
        );
}
