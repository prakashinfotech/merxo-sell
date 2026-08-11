using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>Public (no auth) endpoint — active banners grouped by homepage slot.</summary>
[Route("api/offer-banners")]
public class OfferBannersController(IOfferBannerService service) : BaseApiController
{
    /// <summary>
    /// GET /api/offer-banners
    /// Returns active banners grouped by slot: { hero, midLeft, midRight, strip }.
    /// Only banners that are IsActive and within their date window are included.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPublicBanners() =>
        Success(await service.GetPublicBannersAsync());
}
