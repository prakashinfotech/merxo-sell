using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Categories;

/// <summary>Flat admin row showing a category and counts for product / sub-category usage.</summary>
public record AdminCategoryDto(
    int     CategoryId,
    int?    ParentCategoryId,
    string? ParentName,
    string  Name,
    string  Slug,
    string? ImageUrl,
    int     SortOrder,
    bool    IsActive,
    int     ProductCount,
    int     SubCategoryCount
);

public record CreateCategoryDto(
    [Required, MaxLength(120)]                string  Name,
    [MaxLength(140)]                          string? Slug,
    [MaxLength(500)]                          string? ImageUrl,
                                              int?    ParentCategoryId,
    [Range(0, 1_000)]                         int     SortOrder = 0,
                                              bool    IsActive  = true
);

public record UpdateCategoryDto(
    [Required, MaxLength(120)]                string  Name,
    [MaxLength(140)]                          string? Slug,
    [MaxLength(500)]                          string? ImageUrl,
                                              int?    ParentCategoryId,
    [Range(0, 1_000)]                         int     SortOrder,
                                              bool    IsActive
);
