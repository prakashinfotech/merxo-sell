using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Reviews;

public record ReviewDto(
    int      ReviewId,
    int      ProductId,
    int      UserId,
    string   UserName,
    int      Rating,
    string?  Comment,
    DateTime CreatedAt
);

public record PortalReviewDto(
    int      ReviewId,
    int      ProductId,
    string   ProductName,
    int      UserId,
    string   UserName,
    string   UserEmail,
    int      Rating,
    string?  Comment,
    DateTime CreatedAt,
    int?     SellerId,
    string?  StoreName
);

public record MyReviewDto(
    int      ReviewId,
    int      ProductId,
    string   ProductName,
    string?  ProductImageUrl,
    int      Rating,
    string?  Comment,
    DateTime CreatedAt
);

public record CreateReviewDto(
    [Required] int ProductId,
    [Required, Range(1, 5)] int Rating,
    [MaxLength(1000)] string? Comment
);
