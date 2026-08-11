using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IOrderStatusHistoryRepository
{
    Task<OrderStatusHistory> RecordAsync(OrderStatusHistory entry);

    Task<IEnumerable<OrderStatusHistory>> GetForOrderAsync(int orderId);

    /// <summary>Latest entry, useful for "current status changed by/at".</summary>
    Task<OrderStatusHistory?> GetLatestAsync(int orderId);
}
