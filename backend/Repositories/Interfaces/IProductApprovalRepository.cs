using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IProductApprovalRepository
{
    Task<IEnumerable<Product>> GetPendingQueueAsync();
    Task ApproveAsync(int productId, int reviewerUserId, string? note);
    Task RejectAsync(int productId, int reviewerUserId, string note);
    Task<IEnumerable<ProductApprovalLog>> GetHistoryAsync(int? productId, int page, int pageSize);
}
