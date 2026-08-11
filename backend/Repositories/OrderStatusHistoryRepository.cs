using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
{
    private readonly AppDbContext _db;

    public OrderStatusHistoryRepository(AppDbContext db) => _db = db;

    public async Task<OrderStatusHistory> RecordAsync(OrderStatusHistory entry)
    {
        _db.OrderStatusHistories.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<IEnumerable<OrderStatusHistory>> GetForOrderAsync(int orderId)
        => await _db.OrderStatusHistories
            .AsNoTracking()
            .Include(h => h.Changer)
            .Where(h => h.OrderId == orderId)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();

    public async Task<OrderStatusHistory?> GetLatestAsync(int orderId)
        => await _db.OrderStatusHistories
            .AsNoTracking()
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.CreatedAt)
            .FirstOrDefaultAsync();
}
