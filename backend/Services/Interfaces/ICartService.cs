using MerxoSell.API.DTOs.Cart;

namespace MerxoSell.API.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddItemAsync(int userId, AddToCartDto dto);
    Task<CartDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto);
    Task<CartDto> RemoveItemAsync(int userId, int cartItemId);
    Task ClearCartAsync(int userId);
}
