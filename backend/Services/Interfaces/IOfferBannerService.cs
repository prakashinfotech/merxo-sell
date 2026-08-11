using MerxoSell.API.DTOs.Banners;

namespace MerxoSell.API.Services.Interfaces;

public interface IOfferBannerService
{
    /// <summary>Public endpoint: active banners grouped by slot.</summary>
    Task<BannersBySlotDto>     GetPublicBannersAsync();

    /// <summary>Admin endpoint: all banners regardless of status.</summary>
    Task<IEnumerable<OfferBannerDto>> GetAllAsync();

    Task<OfferBannerDto?>      GetByIdAsync(int id);
    Task<OfferBannerDto>       CreateAsync(CreateOfferBannerDto dto);
    Task<OfferBannerDto>       UpdateAsync(int id, UpdateOfferBannerDto dto);
    Task                       DeleteAsync(int id);
    Task<bool>                 SetActiveAsync(int id, bool isActive);
}
