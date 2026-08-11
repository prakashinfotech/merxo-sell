using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Approval;

public record ApprovalQueueItemDto(
    int      ProductId,
    string   Name,
    string   Slug,
    decimal  BasePrice,
    string   Status,
    string   SellerStoreName,
    string   SellerEmail,
    string   CategoryName,
    string?  PrimaryImageUrl,
    DateTime CreatedAt
);

public record ApproveProductDto(
    [MaxLength(1000)] string? Note
);

public record RejectProductDto(
    [Required, MaxLength(1000)] string Note
);

public record ApprovalLogDto(
    int      LogId,
    int      ProductId,
    string   ProductName,
    string?  ReviewerEmail,
    string   OldStatus,
    string   NewStatus,
    string?  Note,
    DateTime CreatedAt
);
