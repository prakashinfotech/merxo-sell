using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

/// <summary>Admin queries over Buyer-role users.</summary>
public interface ICustomerRepository
{
    Task<IEnumerable<User>>     GetAllBuyersAsync(string? search, bool? isActive);
    Task<User?>                 GetBuyerByIdAsync(int userId);
    Task<int>                   GetOrderCountAsync(int userId);
    Task<decimal>               GetTotalSpentCadAsync(int userId);
    Task<int>                   GetAddressCountAsync(int userId);
    Task<IEnumerable<Address>>  GetAddressesAsync(int userId);
    Task<(string? Status, DateTime? CreatedAt)> GetLastOrderAsync(int userId);
    Task<bool>                  UpdateAsync(User user);
    Task<bool>                  SetStatusAsync(int userId, bool isActive);
    Task<bool>                  SoftDeleteAsync(int userId);
}
