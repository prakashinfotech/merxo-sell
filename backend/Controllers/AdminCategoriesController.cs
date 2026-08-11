using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Categories;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>SuperAdmin-only category management. Hierarchy = optional ParentCategoryId.</summary>
[Route("api/admin/categories")]
[Authorize(Roles = "SuperAdmin")]
public class AdminCategoriesController : BaseApiController
{
    private readonly ICategoryRepository                  _repo;
    private readonly ILogger<AdminCategoriesController>   _logger;

    public AdminCategoriesController(
        ICategoryRepository repo,
        ILogger<AdminCategoriesController> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    /// <summary>GET /api/admin/categories — flat list of every category with parent name + counts.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = (await _repo.GetAllAsync()).ToList();

        var result = new List<AdminCategoryDto>(all.Count);
        foreach (var c in all)
        {
            result.Add(new AdminCategoryDto(
                c.CategoryId,
                c.ParentCategoryId,
                c.ParentCategory?.Name,
                c.Name,
                c.Slug,
                c.ImageUrl,
                c.SortOrder,
                c.IsActive,
                await _repo.GetProductCountAsync(c.CategoryId),
                await _repo.GetSubCategoryCountAsync(c.CategoryId)));
        }
        return Ok(result);
    }

    /// <summary>GET /api/admin/categories/{id} — single category detail.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _repo.GetByIdAsync(id);
        if (c is null) return NotFound(new { error = $"Category {id} not found." });

        return Ok(new AdminCategoryDto(
            c.CategoryId, c.ParentCategoryId, c.ParentCategory?.Name,
            c.Name, c.Slug, c.ImageUrl, c.SortOrder, c.IsActive,
            await _repo.GetProductCountAsync(c.CategoryId),
            await _repo.GetSubCategoryCountAsync(c.CategoryId)));
    }

    /// <summary>POST /api/admin/categories — create a new category or sub-category.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(dto.Name)
            : dto.Slug.Trim().ToLowerInvariant();

        if (await _repo.SlugExistsAsync(slug))
            return Conflict(new { error = $"A category with slug '{slug}' already exists." });

        if (dto.ParentCategoryId.HasValue)
        {
            var parent = await _repo.GetByIdAsync(dto.ParentCategoryId.Value);
            if (parent is null) return BadRequest(new { error = "Parent category not found." });
        }

        var entity = new Category
        {
            Name             = dto.Name.Trim(),
            Slug             = slug,
            ImageUrl         = dto.ImageUrl,
            ParentCategoryId = dto.ParentCategoryId,
            SortOrder        = dto.SortOrder,
            IsActive         = dto.IsActive
        };

        var created = await _repo.CreateAsync(entity);
        _logger.LogInformation("Category {CategoryId} '{Name}' created", created.CategoryId, created.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.CategoryId },
            new AdminCategoryDto(created.CategoryId, created.ParentCategoryId, null,
                created.Name, created.Slug, created.ImageUrl, created.SortOrder, created.IsActive, 0, 0));
    }

    /// <summary>PUT /api/admin/categories/{id} — update category fields.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _repo.GetByIdAsync(id);
        if (existing is null) return NotFound(new { error = $"Category {id} not found." });

        if (dto.ParentCategoryId == id)
            return BadRequest(new { error = "A category cannot be its own parent." });

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(dto.Name)
            : dto.Slug.Trim().ToLowerInvariant();

        if (await _repo.SlugExistsAsync(slug, excludeId: id))
            return Conflict(new { error = $"A category with slug '{slug}' already exists." });

        if (dto.ParentCategoryId.HasValue)
        {
            var parent = await _repo.GetByIdAsync(dto.ParentCategoryId.Value);
            if (parent is null) return BadRequest(new { error = "Parent category not found." });
        }

        var updated = await _repo.UpdateAsync(new Category
        {
            CategoryId       = id,
            Name             = dto.Name.Trim(),
            Slug             = slug,
            ImageUrl         = dto.ImageUrl,
            ParentCategoryId = dto.ParentCategoryId,
            SortOrder        = dto.SortOrder,
            IsActive         = dto.IsActive
        });

        _logger.LogInformation("Category {CategoryId} updated", id);
        return Ok(new AdminCategoryDto(
            updated.CategoryId, updated.ParentCategoryId, null,
            updated.Name, updated.Slug, updated.ImageUrl,
            updated.SortOrder, updated.IsActive,
            await _repo.GetProductCountAsync(id),
            await _repo.GetSubCategoryCountAsync(id)));
    }

    /// <summary>
    /// DELETE /api/admin/categories/{id} — soft-delete (deactivate) by default.
    /// Refuses if products or sub-categories still reference it.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null) return NotFound(new { error = $"Category {id} not found." });

        var products    = await _repo.GetProductCountAsync(id);
        var subCategs   = await _repo.GetSubCategoryCountAsync(id);

        if (products > 0)
            return BadRequest(new { error = $"Category has {products} product(s) — reassign or remove them first." });

        if (subCategs > 0)
            return BadRequest(new { error = $"Category has {subCategs} sub-categor{(subCategs == 1 ? "y" : "ies")} — remove them first." });

        await _repo.SetActiveAsync(id, false);
        _logger.LogInformation("Category {CategoryId} deactivated", id);
        return NoContent();
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
        var chars = slug.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        slug = new string(chars);
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}
