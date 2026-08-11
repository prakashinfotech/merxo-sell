using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByEmailAsync(string email) =>
        await _db.Users
                 .Include(u => u.Role)
                 .Include(u => u.Seller)   // needed for SellerLoginAsync to validate the store
                 .FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        // Explicitly load Role so GenerateToken can read RoleName without a second query
        await _db.Entry(user).Reference(u => u.Role).LoadAsync();
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(string email) =>
        await _db.Users.AnyAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int userId) =>
        await _db.Users
            .Include(u => u.Role)
            .Include(u => u.Seller)
            .FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }
}
