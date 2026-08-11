using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Category>> GetAllActiveAsync()
        => await _db.Categories
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

    public async Task<IEnumerable<Category>> GetAllAsync()
        => await _db.Categories
                    .AsNoTracking()
                    .Include(c => c.ParentCategory)
                    .OrderBy(c => c.ParentCategoryId)
                    .ThenBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

    public async Task<Category?> GetByIdAsync(int categoryId)
        => await _db.Categories
                    .AsNoTracking()
                    .Include(c => c.ParentCategory)
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
        => await _db.Categories
                    .AnyAsync(c => c.Slug == slug && (excludeId == null || c.CategoryId != excludeId));

    public async Task<int> GetProductCountAsync(int categoryId)
        => await _db.Products.CountAsync(p => p.CategoryId == categoryId);

    public async Task<int> GetSubCategoryCountAsync(int categoryId)
        => await _db.Categories.CountAsync(c => c.ParentCategoryId == categoryId);

    public async Task<Category> CreateAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        var existing = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId)
            ?? throw new KeyNotFoundException($"Category {category.CategoryId} not found.");

        existing.Name             = category.Name;
        existing.Slug             = category.Slug;
        existing.ImageUrl         = category.ImageUrl;
        existing.ParentCategoryId = category.ParentCategoryId;
        existing.SortOrder        = category.SortOrder;
        existing.IsActive         = category.IsActive;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> SetActiveAsync(int categoryId, bool isActive)
    {
        var c = await _db.Categories.FindAsync(categoryId);
        if (c is null) return false;

        c.IsActive = isActive;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteAsync(int categoryId)
    {
        var c = await _db.Categories.FindAsync(categoryId);
        if (c is null) return false;

        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
        return true;
    }
}
