using MerxoSell.API.DTOs.Banners;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class OfferBannerService(
    IOfferBannerRepository repo,
    ILogger<OfferBannerService> logger) : IOfferBannerService
{
    private static readonly HashSet<string> ValidSlots =
        new(StringComparer.OrdinalIgnoreCase) { "Hero", "MidLeft", "MidRight", "Strip" };

    private const int SlotWarningThreshold = 5;

    // ── Public ───────────────────────────────────────────────────────────────

    public async Task<BannersBySlotDto> GetPublicBannersAsync()
    {
        var hero     = await repo.GetActiveBySlotAsync("Hero");
        var midLeft  = await repo.GetActiveBySlotAsync("MidLeft");
        var midRight = await repo.GetActiveBySlotAsync("MidRight");
        var strip    = await repo.GetActiveBySlotAsync("Strip");

        return new BannersBySlotDto(
            hero    .Select(ToDto),
            midLeft .Select(ToDto),
            midRight.Select(ToDto),
            strip   .Select(ToDto)
        );
    }

    // ── Admin ────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<OfferBannerDto>> GetAllAsync() =>
        (await repo.GetAllAsync()).Select(ToDto);

    public async Task<OfferBannerDto?> GetByIdAsync(int id)
    {
        var banner = await repo.GetByIdAsync(id);
        return banner is null ? null : ToDto(banner);
    }

    public async Task<OfferBannerDto> CreateAsync(CreateOfferBannerDto dto)
    {
        Validate(dto.Slot, dto.ImageUrl, dto.CtaUrl, dto.StartsAt, dto.EndsAt);

        var banner = new OfferBanner
        {
            Slot            = NormalizeSlot(dto.Slot),
            Title           = dto.Title.Trim(),
            Subtitle        = dto.Subtitle?.Trim(),
            BadgeText       = dto.BadgeText?.Trim(),
            ImageUrl        = dto.ImageUrl.Trim(),
            SideImageUrl    = dto.SideImageUrl?.Trim(),
            CtaLabel        = dto.CtaLabel?.Trim(),
            CtaUrl          = dto.CtaUrl?.Trim(),
            SecondaryLabel  = dto.SecondaryLabel?.Trim(),
            SecondaryUrl    = dto.SecondaryUrl?.Trim(),
            BackgroundColor = dto.BackgroundColor?.Trim(),
            TextColor       = dto.TextColor?.Trim(),
            LinkedProductId  = dto.LinkedProductId,
            LinkedCategoryId = dto.LinkedCategoryId,
            StartsAt        = dto.StartsAt,
            EndsAt          = dto.EndsAt,
            SortOrder       = dto.SortOrder,
            IsActive        = dto.IsActive,
            CreatedAt       = DateTime.UtcNow,
        };

        await WarnIfSlotCrowdedAsync(banner.Slot);
        var created = await repo.CreateAsync(banner);
        return ToDto(created);
    }

    public async Task<OfferBannerDto> UpdateAsync(int id, UpdateOfferBannerDto dto)
    {
        var banner = await repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Banner {id} not found.");

        Validate(dto.Slot, dto.ImageUrl, dto.CtaUrl, dto.StartsAt, dto.EndsAt);

        banner.Slot            = NormalizeSlot(dto.Slot);
        banner.Title           = dto.Title.Trim();
        banner.Subtitle        = dto.Subtitle?.Trim();
        banner.BadgeText       = dto.BadgeText?.Trim();
        banner.ImageUrl        = dto.ImageUrl.Trim();
        banner.SideImageUrl    = dto.SideImageUrl?.Trim();
        banner.CtaLabel        = dto.CtaLabel?.Trim();
        banner.CtaUrl          = dto.CtaUrl?.Trim();
        banner.SecondaryLabel  = dto.SecondaryLabel?.Trim();
        banner.SecondaryUrl    = dto.SecondaryUrl?.Trim();
        banner.BackgroundColor = dto.BackgroundColor?.Trim();
        banner.TextColor       = dto.TextColor?.Trim();
        banner.LinkedProductId  = dto.LinkedProductId;
        banner.LinkedCategoryId = dto.LinkedCategoryId;
        banner.StartsAt        = dto.StartsAt;
        banner.EndsAt          = dto.EndsAt;
        banner.SortOrder       = dto.SortOrder;
        banner.IsActive        = dto.IsActive;
        banner.UpdatedAt       = DateTime.UtcNow;

        var updated = await repo.UpdateAsync(banner);
        return ToDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Banner {id} not found.");
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        var result = await repo.SetActiveAsync(id, isActive);
        if (!result)
            throw new KeyNotFoundException($"Banner {id} not found.");
        return isActive;
    }

    // ── Validation helpers ───────────────────────────────────────────────────

    private static void Validate(
        string slot, string imageUrl, string? ctaUrl,
        DateTime? startsAt, DateTime? endsAt)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("ImageUrl is required.");

        if (!ValidSlots.Contains(slot))
            throw new ArgumentException(
                $"Slot '{slot}' is invalid. Must be one of: {string.Join(", ", ValidSlots)}.");

        if (startsAt.HasValue && endsAt.HasValue && endsAt <= startsAt)
            throw new ArgumentException("EndsAt must be after StartsAt.");

        if (!string.IsNullOrWhiteSpace(ctaUrl)
            && !ctaUrl.StartsWith('/')
            && !ctaUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "CtaUrl must start with '/' (internal) or 'http' (external).");
        }
    }

    private async Task WarnIfSlotCrowdedAsync(string slot)
    {
        var all = await repo.GetAllAsync();
        var activeInSlot = all.Count(b => b.Slot == slot && b.IsActive);
        if (activeInSlot >= SlotWarningThreshold)
        {
            logger.LogWarning(
                "Slot '{Slot}' already has {Count} active banners. " +
                "Consider deactivating older ones to avoid visual clutter.",
                slot, activeInSlot);
        }
    }

    private static string NormalizeSlot(string slot) =>
        slot switch
        {
            var s when string.Equals(s, "midleft",  StringComparison.OrdinalIgnoreCase) => "MidLeft",
            var s when string.Equals(s, "midright", StringComparison.OrdinalIgnoreCase) => "MidRight",
            var s when string.Equals(s, "strip",    StringComparison.OrdinalIgnoreCase) => "Strip",
            _ => "Hero"
        };

    // ── Mapping ──────────────────────────────────────────────────────────────

    private static OfferBannerDto ToDto(OfferBanner b) => new(
        b.BannerId,
        b.Slot,
        b.Title,
        b.Subtitle,
        b.BadgeText,
        b.ImageUrl,
        b.SideImageUrl,
        b.CtaLabel,
        b.CtaUrl,
        b.SecondaryLabel,
        b.SecondaryUrl,
        b.BackgroundColor,
        b.TextColor,
        b.LinkedProductId,
        b.LinkedProduct?.Name,
        b.LinkedCategoryId,
        b.LinkedCategory?.Name,
        b.StartsAt,
        b.EndsAt,
        b.SortOrder,
        b.IsActive,
        b.CreatedAt,
        b.UpdatedAt
    );
}
