using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Seller;

public record SellerVariantImageDto(
    int     ImageId,
    string  ImageUrl,
    bool    IsPrimary,
    int     SortOrder
);

public record SellerVariantDto(
    int      VariantId,
    string?  Color,
    string?  Size,
    string?  SKU,
    decimal  PriceDelta,
    int      Stock,
    bool     IsActive,
    bool     IsDefault,
    List<SellerVariantImageDto> Images
);

public record UpsertVariantDto(
    [MaxLength(50)]                 string?  Color,
    [MaxLength(50)]                 string?  Size,
    [MaxLength(100)]                string?  SKU,
                                    decimal  PriceDelta,
    [Range(0, int.MaxValue)]        int      Stock,
                                    bool     IsActive = true,
                                    bool     IsDefault = false,
                                    // Image URLs to attach to this variant. Order matters; first = primary variant photo.
                                    IList<string>? ImageUrls = null
);

/// <summary>
/// Bulk-replace payload: the seller submits the full variant list every save,
/// and the server diffs it against the existing rows (delete missing, update
/// matching, insert new). Avoids fiddly PATCH semantics for the typical
/// "edit a product's variants" workflow on the seller dashboard.
/// </summary>
public record BulkUpsertVariantsDto(
    [Required] IList<UpsertVariantWithIdDto> Variants
);

public record UpsertVariantWithIdDto(
    int?    VariantId,                           // null => new row
    [MaxLength(50)]  string?  Color,
    [MaxLength(50)]  string?  Size,
    [MaxLength(100)] string?  SKU,
                     decimal  PriceDelta,
    [Range(0, int.MaxValue)] int Stock,
                     bool     IsActive = true,
                     bool     IsDefault = false,
    IList<string>?            ImageUrls = null
);
