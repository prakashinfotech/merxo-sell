using MerxoSell.API.DTOs.Orders;

namespace MerxoSell.API.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto>               CreateOrderAsync(int userId, CreateOrderDto dto);
    Task<OrderDto?>              GetOrderAsync(int orderId, int? userId = null);
    Task<IEnumerable<OrderDto>>  GetUserOrdersAsync(int userId);
    Task<IEnumerable<OrderDto>>  GetAllOrdersAsync();

    /// <summary>Records a status-history row, persists the change, and emails the buyer.</summary>
    Task UpdateOrderStatusAsync(
        int orderId, string status, string? note = null,
        int? changedByUserId = null, string? changedByRole = null);

    /// <summary>Buyer/Admin cancellation — reason is required and stored on the order.</summary>
    Task CancelOrderAsync(
        int orderId, string cancellationReason,
        int? changedByUserId = null, string? changedByRole = null);

    Task<IEnumerable<OrderStatusHistoryDto>> GetStatusHistoryAsync(int orderId);
}
