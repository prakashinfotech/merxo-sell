using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IOfferBannerRepository
{
    /// <summary>
    /// Returns active banners for a specific slot whose date window includes now,
    /// ordered by SortOrder ascending.
    /// </summary>
    Task<IEnumerable<OfferBanner>> GetActiveBySlotAsync(string slot);

    /// <summary>Returns every banner regardless of status (admin view).</summary>
    Task<IEnumerable<OfferBanner>> GetAllAsync();

    Task<OfferBanner?> GetByIdAsync(int bannerId);
    Task<OfferBanner>  CreateAsync(OfferBanner banner);
    Task<OfferBanner>  UpdateAsync(OfferBanner banner);
    Task<bool>         DeleteAsync(int bannerId);
    Task<bool>         SetActiveAsync(int bannerId, bool isActive);
}
