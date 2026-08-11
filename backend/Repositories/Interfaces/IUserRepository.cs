using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User>  CreateAsync(User user);
    Task<bool>  ExistsByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<User> UpdateAsync(User user);
}
