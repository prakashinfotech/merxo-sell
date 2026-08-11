using MerxoSell.API.DTOs.Cart;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepo;

    public CartService(ICartRepository cartRepo)
    {
        _cartRepo = cartRepo;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);
        return MapToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(int userId, AddToCartDto dto)
    {
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);

        // Check stock
        int maxStock = dto.VariantId.HasValue 
            ? (await _cartRepo.GetVariantStockAsync(dto.VariantId.Value) ?? 0) 
            : await _cartRepo.GetProductStockAsync(dto.ProductId);

        if (dto.Quantity > maxStock)
            throw new InvalidOperationException($"Cannot add {dto.Quantity} items. Only {maxStock} in stock.");

        var existingItem = await _cartRepo.GetCartItemAsync(cart.CartId, dto.ProductId, dto.VariantId);

        if (existingItem != null)
        {
            if (existingItem.Quantity + dto.Quantity > maxStock)
                throw new InvalidOperationException($"Cannot add more items. Max stock is {maxStock}.");

            existingItem.Quantity += dto.Quantity;
            await _cartRepo.UpdateItemAsync(existingItem);
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.CartId,
                ProductId = dto.ProductId,
                VariantId = dto.VariantId,
                Quantity = dto.Quantity
            };
            await _cartRepo.AddItemAsync(newItem);
        }

        // Reload cart
        cart = await _cartRepo.GetCartByUserIdAsync(userId);
        return MapToDto(cart);
    }

    public async Task<CartDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
    {
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.CartItemId == cartItemId);

        if (item == null)
            throw new KeyNotFoundException("Cart item not found.");

        int maxStock = item.VariantId.HasValue 
            ? (await _cartRepo.GetVariantStockAsync(item.VariantId.Value) ?? 0) 
            : await _cartRepo.GetProductStockAsync(item.ProductId);

        if (dto.Quantity > maxStock)
            throw new InvalidOperationException($"Cannot set quantity to {dto.Quantity}. Only {maxStock} in stock.");

        if (dto.Quantity <= 0)
        {
            await _cartRepo.RemoveItemAsync(item);
        }
        else
        {
            item.Quantity = dto.Quantity;
            await _cartRepo.UpdateItemAsync(item);
        }

        cart = await _cartRepo.GetCartByUserIdAsync(userId);
        return MapToDto(cart);
    }

    public async Task<CartDto> RemoveItemAsync(int userId, int cartItemId)
    {
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.CartItemId == cartItemId);

        if (item != null)
        {
            await _cartRepo.RemoveItemAsync(item);
        }

        cart = await _cartRepo.GetCartByUserIdAsync(userId);
        return MapToDto(cart);
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);
        await _cartRepo.ClearCartAsync(cart.CartId);
    }

    private CartDto MapToDto(Cart cart)
    {
        var items = cart.Items.Select(i => 
        {
            decimal price = i.Product.SalePrice ?? i.Product.BasePrice;
            if (i.Variant != null) price += i.Variant.PriceDelta;

            string? variantInfo = null;
            if (i.Variant != null)
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(i.Variant.Color)) parts.Add($"Color: {i.Variant.Color}");
                if (!string.IsNullOrEmpty(i.Variant.Size)) parts.Add($"Size: {i.Variant.Size}");
                if (parts.Count > 0) variantInfo = string.Join(", ", parts);
            }

            return new CartItemDto(
                i.CartItemId,
                i.ProductId,
                i.VariantId,
                i.Product.Name,
                i.Product.Images.FirstOrDefault()?.ImageUrl,
                variantInfo,
                price,
                i.Quantity,
                i.Variant != null ? i.Variant.Stock : i.Product.Stock
            );
        }).ToList();

        decimal total = items.Sum(i => i.UnitPriceCAD * i.Quantity);

        return new CartDto(cart.CartId, cart.UserId, total, items);
    }
}
