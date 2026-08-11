using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface ISellerRepository
{
    Task<Seller?> GetByIdAsync(int sellerId);
    Task<Seller?> GetByUserIdAsync(int userId);
    Task<IEnumerable<Seller>> GetAllAsync(string? search, bool? isActive);
    Task<Seller> CreateAsync(Seller seller);
    Task<Seller> UpdateAsync(Seller seller);
    Task<bool> SetStatusAsync(int sellerId, bool isActive);
    Task<bool> ExistsByUserIdAsync(int userId);
    Task<bool> SoftDeleteAsync(int sellerId);
}
