using MerxoSell.API.DTOs.BrowsingHistory;

namespace MerxoSell.API.Services.Interfaces;

public interface IBrowsingHistoryService
{
    Task RecordViewAsync(int? userId, int productId);
    Task<IReadOnlyList<BrowsingHistoryDto>> GetForUserAsync(int userId, int page, int pageSize);
    Task<bool> DeleteAsync(int userId, int browsingHistoryId);
    Task ClearAsync(int userId);
}
