using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Constants;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<User>> GetAllBuyersAsync(string? search, bool? isActive)
    {
        var q = _db.Users
            .AsNoTracking()
            .Where(u => u.Role.RoleName == AppRoles.Buyer && !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(u => u.FullName.Contains(s) || u.Email.Contains(s));
        }

        if (isActive.HasValue)
            q = q.Where(u => u.IsActive == isActive.Value);

        return await q.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<User?> GetBuyerByIdAsync(int userId) =>
        await _db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.Role.RoleName == AppRoles.Buyer && !u.IsDeleted);

    public async Task<int> GetOrderCountAsync(int userId) =>
        await _db.Orders.CountAsync(o => o.UserId == userId);

    public async Task<decimal> GetTotalSpentCadAsync(int userId) =>
        await _db.Orders
            .Where(o => o.UserId == userId && o.Status != "Cancelled" && o.Status != "Refunded")
            .SumAsync(o => (decimal?)o.TotalAmountCAD) ?? 0m;

    public async Task<int> GetAddressCountAsync(int userId) =>
        await _db.Addresses.CountAsync(a => a.UserId == userId);

    public async Task<IEnumerable<Address>> GetAddressesAsync(int userId) =>
        await _db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.AddressId)
            .ToListAsync();

    public async Task<(string? Status, DateTime? CreatedAt)> GetLastOrderAsync(int userId)
    {
        var last = await _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new { o.Status, o.CreatedAt })
            .FirstOrDefaultAsync();
        return (last?.Status, last?.CreatedAt);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var existing = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == user.UserId);
        if (existing is null || existing.Role.RoleName != AppRoles.Buyer) return false;

        existing.FullName          = user.FullName;
        existing.Phone             = user.Phone;
        existing.PreferredCurrency = user.PreferredCurrency;
        existing.IsActive          = user.IsActive;
        existing.UpdatedAt         = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetStatusAsync(int userId, bool isActive)
    {
        var existing = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        if (existing is null || existing.Role.RoleName != AppRoles.Buyer) return false;

        existing.IsActive  = isActive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteAsync(int userId)
    {
        var existing = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        if (existing is null || existing.Role.RoleName != AppRoles.Buyer) return false;

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
