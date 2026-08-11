using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(Order order);
    Task<Order?> GetByIdAsync(int orderId, int? userId = null);
    Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task UpdateOrderStatusAsync(int orderId, string status);
}
