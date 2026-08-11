using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Categories;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>Public read-only category tree — no authentication required.</summary>
[Route("api/categories")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>GET /api/categories — returns the full category tree (root → children).</summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetTreeAsync()
    {
        var tree = await _categoryService.GetTreeAsync();
        return Ok(tree);
    }
}
