using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IBrowsingHistoryRepository
{
    Task RecordViewAsync(int? userId, int productId);
    Task<IEnumerable<BrowsingHistory>> GetForUserAsync(int userId, int skip, int take);
    Task<bool> DeleteAsync(int userId, int browsingHistoryId);
    Task ClearForUserAsync(int userId);
}
