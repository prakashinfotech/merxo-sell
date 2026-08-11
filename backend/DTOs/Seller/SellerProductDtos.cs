using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Seller;

// ── Seller Product DTOs ───────────────────────────────────────────────────────

public record SellerProductDetailDto(
    int      ProductId,
    string   Name,
    string?  Description,
    int      CategoryId,
    string   CategoryName,
    int?     ParentCategoryId,
    string?  ParentCategoryName,
    int?     ManufacturerId,
    string?  ManufacturerName,
    decimal  BasePrice,
    decimal? SalePrice,
    int      Stock,
    string   Status,
    string?  ApprovalNote,
    bool     IsActive,
    string?  PrimaryImageUrl,
    DateTime CreatedAt,
    IList<CreateProductImageDto>? Images = null,
    IList<string>? Colors = null,
    IList<string>? Sizes = null,
    // True when the category (or its parent) is flagged Fashion — unlocks the variant matrix UI.
    bool IsFashion = false
);

public record SellerProductListDto(
    int      ProductId,
    string   Name,
    string   Slug,
    decimal  BasePrice,
    decimal? SalePrice,
    int      Stock,
    string   Status,
    string?  ApprovalNote,
    string   CategoryName,
    string?  ManufacturerName,
    string?  PrimaryImageUrl,
    int      ViewCount,
    DateTime CreatedAt
);

public record CreateSellerProductDto(
    [Required, MaxLength(300)]  string   Name,
    [Required]                  int      CategoryId,
    [MaxLength(2000)]           string?  Description,
    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
                                decimal  BasePrice,
    [Range(0.01, double.MaxValue)] decimal? SalePrice,
    [Required, Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
                                int      Stock,
    int?                                 ManufacturerId,
    IList<CreateProductImageDto>?        Images,
    IList<CreateProductVariantDto>?      Variants,
    IList<string>?                       Colors,
    IList<string>?                       Sizes
);

public record UpdateSellerProductDto(
    [Required, MaxLength(300)]  string   Name,
    [Required]                  int      CategoryId,
    [MaxLength(2000)]           string?  Description,
    [Required, Range(0.01, double.MaxValue)] decimal BasePrice,
    [Range(0.01, double.MaxValue)] decimal? SalePrice,
    [Required, Range(0, int.MaxValue)] int Stock,
    int?                                 ManufacturerId,
    IList<CreateProductImageDto>?        Images,
    IList<string>?                       Colors,
    IList<string>?                       Sizes
);

public record CreateProductImageDto(
    [Required, Url] string ImageUrl,
    bool IsPrimary = false
);

public record CreateProductVariantDto(
    [MaxLength(50)] string? Color,
    [MaxLength(50)] string? Size,
    [MaxLength(100)] string? SKU,
    decimal                            PriceDelta,
    [Range(0, int.MaxValue)] int       Stock
);
