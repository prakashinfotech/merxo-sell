using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);
    Task<Role?> GetByIdAsync(int roleId);
    Task<IEnumerable<Role>> GetAllAsync();
}
