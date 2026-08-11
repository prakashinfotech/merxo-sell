namespace MerxoSell.API.DTOs.BrowsingHistory;

public record AddBrowsingHistoryDto(int ProductId);

public record BrowsingHistoryDto(
    int BrowsingHistoryId,
    int ProductId,
    string ProductName,
    string? PrimaryImageUrl,
    decimal BasePrice,
    decimal? SalePrice,
    DateTime ViewedAt
);
