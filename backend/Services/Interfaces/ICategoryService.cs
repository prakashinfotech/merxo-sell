using MerxoSell.API.DTOs.Categories;

namespace MerxoSell.API.Services.Interfaces;

public interface ICategoryService
{
    /// <summary>Returns all active categories as a nested tree (root → children).</summary>
    Task<IEnumerable<CategoryDto>> GetTreeAsync();
}
