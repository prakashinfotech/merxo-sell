using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class OfferBannerRepository(AppDbContext db) : IOfferBannerRepository
{
    /// <inheritdoc/>
    public async Task<IEnumerable<OfferBanner>> GetActiveBySlotAsync(string slot)
    {
        var now = DateTime.UtcNow;
        return await db.OfferBanners
            .Where(b => b.Slot == slot
                     && b.IsActive
                     && (b.StartsAt == null || b.StartsAt <= now)
                     && (b.EndsAt   == null || b.EndsAt   >= now))
            .OrderBy(b => b.SortOrder)
            .Include(b => b.LinkedProduct)
            .Include(b => b.LinkedCategory)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<OfferBanner>> GetAllAsync() =>
        await db.OfferBanners
            .OrderBy(b => b.Slot)
            .ThenBy(b => b.SortOrder)
            .Include(b => b.LinkedProduct)
            .Include(b => b.LinkedCategory)
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<OfferBanner?> GetByIdAsync(int bannerId) =>
        await db.OfferBanners
            .Include(b => b.LinkedProduct)
            .Include(b => b.LinkedCategory)
            .FirstOrDefaultAsync(b => b.BannerId == bannerId);

    /// <inheritdoc/>
    public async Task<OfferBanner> CreateAsync(OfferBanner banner)
    {
        db.OfferBanners.Add(banner);
        await db.SaveChangesAsync();
        return banner;
    }

    /// <inheritdoc/>
    public async Task<OfferBanner> UpdateAsync(OfferBanner banner)
    {
        db.OfferBanners.Update(banner);
        await db.SaveChangesAsync();
        return banner;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int bannerId)
    {
        var banner = await db.OfferBanners.FindAsync(bannerId);
        if (banner is null) return false;
        db.OfferBanners.Remove(banner);
        await db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> SetActiveAsync(int bannerId, bool isActive)
    {
        var banner = await db.OfferBanners.FindAsync(bannerId);
        if (banner is null) return false;
        banner.IsActive  = isActive;
        banner.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}
