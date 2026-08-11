using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _db;
    public CartRepository(AppDbContext db) => _db = db;

    public async Task<Cart> GetCartByUserIdAsync(int userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images.Where(img => img.IsPrimary))
            .Include(c => c.Items)
                .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        return cart;
    }

    public async Task<CartItem?> GetCartItemAsync(int cartId, int productId, int? variantId) =>
        await _db.CartItems.FirstOrDefaultAsync(i => 
            i.CartId == cartId && 
            i.ProductId == productId && 
            i.VariantId == variantId);

    public async Task<CartItem> AddItemAsync(CartItem item)
    {
        _db.CartItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<CartItem> UpdateItemAsync(CartItem item)
    {
        _db.CartItems.Update(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task RemoveItemAsync(CartItem item)
    {
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ClearCartAsync(int cartId)
    {
        var items = await _db.CartItems.Where(i => i.CartId == cartId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetProductStockAsync(int productId)
    {
        var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == productId);
        return p?.Stock ?? 0;
    }

    public async Task<int?> GetVariantStockAsync(int variantId)
    {
        var v = await _db.ProductVariants.AsNoTracking().FirstOrDefaultAsync(x => x.VariantId == variantId);
        return v?.Stock;
    }
}
