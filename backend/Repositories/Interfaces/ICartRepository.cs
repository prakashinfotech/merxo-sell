using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface ICartRepository
{
    Task<Cart> GetCartByUserIdAsync(int userId);
    Task<CartItem?> GetCartItemAsync(int cartId, int productId, int? variantId);
    Task<CartItem> AddItemAsync(CartItem item);
    Task<CartItem> UpdateItemAsync(CartItem item);
    Task RemoveItemAsync(CartItem item);
    Task ClearCartAsync(int cartId);
    Task<int> GetProductStockAsync(int productId);
    Task<int?> GetVariantStockAsync(int variantId);
}
