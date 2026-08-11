namespace MerxoSell.API.DTOs.Banners;

// ── Public read DTO (returned to both buyer and admin) ──────────────────────
public record OfferBannerDto(
    int      BannerId,
    string   Slot,
    string   Title,
    string?  Subtitle,
    string?  BadgeText,
    string   ImageUrl,
    string?  SideImageUrl,
    string?  CtaLabel,
    string?  CtaUrl,
    string?  SecondaryLabel,
    string?  SecondaryUrl,
    string?  BackgroundColor,
    string?  TextColor,
    int?     LinkedProductId,
    string?  LinkedProductName,
    int?     LinkedCategoryId,
    string?  LinkedCategoryName,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int      SortOrder,
    bool     IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ── Admin create payload ────────────────────────────────────────────────────
public record CreateOfferBannerDto(
    string   Slot,
    string   Title,
    string?  Subtitle,
    string?  BadgeText,
    string   ImageUrl,
    string?  SideImageUrl,
    string?  CtaLabel,
    string?  CtaUrl,
    string?  SecondaryLabel,
    string?  SecondaryUrl,
    string?  BackgroundColor,
    string?  TextColor,
    int?     LinkedProductId,
    int?     LinkedCategoryId,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int      SortOrder,
    bool     IsActive
);

// ── Admin update payload ────────────────────────────────────────────────────
public record UpdateOfferBannerDto(
    string   Slot,
    string   Title,
    string?  Subtitle,
    string?  BadgeText,
    string   ImageUrl,
    string?  SideImageUrl,
    string?  CtaLabel,
    string?  CtaUrl,
    string?  SecondaryLabel,
    string?  SecondaryUrl,
    string?  BackgroundColor,
    string?  TextColor,
    int?     LinkedProductId,
    int?     LinkedCategoryId,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int      SortOrder,
    bool     IsActive
);

// ── Public grouped response ─────────────────────────────────────────────────
public record BannersBySlotDto(
    IEnumerable<OfferBannerDto> Hero,
    IEnumerable<OfferBannerDto> MidLeft,
    IEnumerable<OfferBannerDto> MidRight,
    IEnumerable<OfferBannerDto> Strip
);

// ── Toggle request ──────────────────────────────────────────────────────────
public record SetActiveRequest(bool IsActive);
