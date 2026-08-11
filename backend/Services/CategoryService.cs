using MerxoSell.API.DTOs.Categories;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;

    public async Task<IEnumerable<CategoryDto>> GetTreeAsync()
    {
        var categories = (await _repo.GetAllActiveAsync()).ToList();
        return BuildTree(categories, parentId: null);
    }

    // Recursively builds the category tree from the flat list.
    private static List<CategoryDto> BuildTree(List<Category> all, int? parentId)
        => all
           .Where(c => c.ParentCategoryId == parentId)
           .Select(c => new CategoryDto
           {
               CategoryId       = c.CategoryId,
               Name             = c.Name,
               Slug             = c.Slug,
               ParentCategoryId = c.ParentCategoryId,
               ImageUrl         = c.ImageUrl,
               IsFashion        = c.IsFashion,
               Children         = BuildTree(all, c.CategoryId)
           })
           .ToList();
}
