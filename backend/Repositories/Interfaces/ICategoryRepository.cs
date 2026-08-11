using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllActiveAsync();
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?>             GetByIdAsync(int categoryId);
    Task<bool>                  SlugExistsAsync(string slug, int? excludeId = null);
    Task<int>                   GetProductCountAsync(int categoryId);
    Task<int>                   GetSubCategoryCountAsync(int categoryId);
    Task<Category>              CreateAsync(Category category);
    Task<Category>              UpdateAsync(Category category);
    Task<bool>                  SetActiveAsync(int categoryId, bool isActive);
    Task<bool>                  HardDeleteAsync(int categoryId);
}
