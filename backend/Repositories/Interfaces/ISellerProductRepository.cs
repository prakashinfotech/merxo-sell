using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface ISellerProductRepository
{
    Task<IEnumerable<Product>> GetBySellerAsync(int sellerId, string? search, string? status);
    Task<Product?> GetByIdAndSellerAsync(int productId, int sellerId);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> SoftDeleteAsync(int productId, int sellerId);
    Task<IEnumerable<Product>> GetPendingAsync();
    Task<Product?> GetByIdForApprovalAsync(int productId);
}
