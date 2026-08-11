using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class BrowsingHistoryRepository : IBrowsingHistoryRepository
{
    private readonly AppDbContext _db;

    public BrowsingHistoryRepository(AppDbContext db) => _db = db;

    public async Task RecordViewAsync(int? userId, int productId)
    {
        BrowsingHistory? existing = null;
        
        if (userId.HasValue)
        {
            existing = await _db.BrowsingHistories
                .FirstOrDefaultAsync(b => b.UserId == userId.Value && b.ProductId == productId);
        }

        if (existing is null)
        {
            _db.BrowsingHistories.Add(new BrowsingHistory
            {
                UserId    = userId,
                ProductId = productId,
                ViewedAt  = DateTime.UtcNow,
            });
        }
        else
        {
            existing.ViewedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<BrowsingHistory>> GetForUserAsync(int userId, int skip, int take)
        => await _db.BrowsingHistories
            .AsNoTracking()
            .Include(b => b.Product).ThenInclude(p => p.Category)
            .Include(b => b.Product).ThenInclude(p => p.Images.Where(i => i.IsPrimary))
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.ViewedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

    public async Task<bool> DeleteAsync(int userId, int browsingHistoryId)
    {
        var entry = await _db.BrowsingHistories
            .FirstOrDefaultAsync(b => b.BrowsingHistoryId == browsingHistoryId && b.UserId == userId);
        if (entry is null) return false;

        _db.BrowsingHistories.Remove(entry);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task ClearForUserAsync(int userId)
    {
        var rows = await _db.BrowsingHistories.Where(b => b.UserId == userId).ToListAsync();
        if (rows.Count == 0) return;

        _db.BrowsingHistories.RemoveRange(rows);
        await _db.SaveChangesAsync();
    }
}
