using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;

    public RoleRepository(AppDbContext db) => _db = db;

    public async Task<Role?> GetByNameAsync(string roleName) =>
        await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);

    public async Task<Role?> GetByIdAsync(int roleId) =>
        await _db.Roles.FindAsync(roleId);

    public async Task<IEnumerable<Role>> GetAllAsync() =>
        await _db.Roles.AsNoTracking().ToListAsync();
}
