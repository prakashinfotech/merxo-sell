using FluentAssertions;
using Moq;
using MerxoSell.API.DTOs.Reviews;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services;

namespace MerxoSell.Tests.Services;

public class ReviewServiceTests : TestBase
{
    private readonly Mock<IReviewRepository> _repo = new();
    private ReviewService BuildSut() => new(_repo.Object);

    private static Review MakeReview(int reviewId, int userId, int productId, byte rating = 5) => new()
    {
        ReviewId  = reviewId,
        UserId    = userId,
        ProductId = productId,
        Rating    = rating,
        Comment   = "Great product",
        CreatedAt = DateTime.UtcNow,
        User      = new User { UserId = userId, FullName = "Reviewer", Email = "r@test.com",
                               Role = new Role { RoleName = "Buyer" }, PasswordHash = "" },
        Product   = new Product { ProductId = productId, Name = "Product", Slug = "product",
                                  Images = new List<ProductImage>(),
                                  Category = new Category { Name = "Cat", Slug = "cat" },
                                  Seller = new Seller { StoreName = "Shop" },
                                  Variants = new List<ProductVariant>(),
                                  Reviews = new List<Review>() }
    };

    // ── AddReview ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddReview_Success_ReturnsReviewDto()
    {
        var review = MakeReview(1, BuyerUser.UserId, 200);

        _repo.Setup(r => r.HasUserReviewedProductAsync(BuyerUser.UserId, 200)).ReturnsAsync(false);
        _repo.Setup(r => r.HasUserPurchasedProductAsync(BuyerUser.UserId, 200)).ReturnsAsync(true);
        _repo.Setup(r => r.CreateReviewAsync(It.IsAny<Review>())).ReturnsAsync(review);
        _repo.Setup(r => r.GetByProductIdAsync(200)).ReturnsAsync(new List<Review> { review });

        var dto    = new CreateReviewDto(ProductId: 200, Rating: 5, Comment: "Great product");
        var result = await BuildSut().AddReviewAsync(BuyerUser.UserId, dto);

        result.ReviewId.Should().Be(1);
        result.Rating.Should().Be(5);
        result.UserName.Should().Be("Reviewer");
    }

    [Fact]
    public async Task AddReview_AlreadyReviewed_ThrowsInvalidOperation()
    {
        _repo.Setup(r => r.HasUserReviewedProductAsync(BuyerUser.UserId, 200)).ReturnsAsync(true);

        var dto = new CreateReviewDto(ProductId: 200, Rating: 4, Comment: null);

        await BuildSut().Invoking(s => s.AddReviewAsync(BuyerUser.UserId, dto))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*already reviewed*");
    }

    [Fact]
    public async Task AddReview_NotPurchased_ThrowsInvalidOperation()
    {
        _repo.Setup(r => r.HasUserReviewedProductAsync(BuyerUser.UserId, 200)).ReturnsAsync(false);
        _repo.Setup(r => r.HasUserPurchasedProductAsync(BuyerUser.UserId, 200)).ReturnsAsync(false);

        var dto = new CreateReviewDto(ProductId: 200, Rating: 5, Comment: null);

        await BuildSut().Invoking(s => s.AddReviewAsync(BuyerUser.UserId, dto))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*must purchase*");
    }

    // ── GetProductReviews ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetProductReviews_ReturnsCorrectlyMappedDtos()
    {
        var reviews = new List<Review>
        {
            MakeReview(1, BuyerUser.UserId, 200, rating: 4),
            MakeReview(2, BuyerUser.UserId, 200, rating: 5),
        };

        _repo.Setup(r => r.GetByProductIdAsync(200)).ReturnsAsync(reviews);

        var result = (await BuildSut().GetProductReviewsAsync(200)).ToList();

        result.Should().HaveCount(2);
        result[0].Rating.Should().Be(4);
        result[1].Rating.Should().Be(5);
    }

    // ── GetMyReviews ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyReviews_ReturnsOnlyUserReviews()
    {
        var reviews = new List<Review>
        {
            MakeReview(10, BuyerUser.UserId, 300, rating: 3)
        };

        _repo.Setup(r => r.GetByUserIdAsync(BuyerUser.UserId)).ReturnsAsync(reviews);

        var result = (await BuildSut().GetMyReviewsAsync(BuyerUser.UserId)).ToList();

        result.Should().HaveCount(1);
        result[0].Rating.Should().Be(3);
    }

    // ── GetPortalReviews ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetPortalReviews_MapsSellerName()
    {
        var review = MakeReview(20, BuyerUser.UserId, 300);
        review.Product.Seller.StoreName = "Awesome Shop";
        review.Product.SellerId = SellerRecord.SellerId;

        _repo.Setup(r => r.GetAllForPortalAsync(null, null)).ReturnsAsync(new List<Review> { review });

        var result = (await BuildSut().GetPortalReviewsAsync(null, null)).ToList();

        result.Should().HaveCount(1);
        result[0].StoreName.Should().Be("Awesome Shop");
    }
}
