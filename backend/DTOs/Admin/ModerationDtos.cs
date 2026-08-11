using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Admin;

public record PendingProductDto(
    int      ProductId,
    string   Name,
    string   Slug,
    decimal  BasePrice,
    decimal? SalePrice,
    int      Stock,
    string   Status,
    string   SellerStoreName,
    string   SellerEmail,
    string   CategoryName,
    string?  PrimaryImageUrl,
    DateTime CreatedAt
);

public record FlaggedReviewDto(
    int      ReviewId,
    int      ProductId,
    string   ProductName,
    string?  ProductImageUrl,
    int      UserId,
    string   AuthorName,
    string   AuthorEmail,
    byte     Rating,
    string?  Comment,
    string?  FlagReason,
    DateTime CreatedAt
);

public record ApproveDto([MaxLength(500)] string? Note);

public record RejectDto([Required, MaxLength(500)] string Reason);

public record FlagReviewDto([Required, MaxLength(500)] string Reason);
