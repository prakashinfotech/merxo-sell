using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Banners;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>
/// SuperAdmin-only CRUD for Offer Banners.
/// Image uploads use the shared POST /api/media/upload endpoint.
/// </summary>
[Route("api/admin/offer-banners")]
[Authorize(Policy = "SuperAdminOnly")]
public class AdminOfferBannersController(
    IOfferBannerService service,
    ILogger<AdminOfferBannersController> logger) : BaseApiController
{
    // ── Read ─────────────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/offer-banners — list all banners (any status).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Success(await service.GetAllAsync());

    /// <summary>GET /api/admin/offer-banners/{id} — single banner.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var banner = await service.GetByIdAsync(id);
        return banner is null
            ? Failure($"Banner {id} not found.", statusCode: 404)
            : Success(banner);
    }

    // ── Write ────────────────────────────────────────────────────────────────

    /// <summary>
    /// POST /api/admin/offer-banners — create a banner.
    /// ImageUrl must be the URL returned by POST /api/media/upload.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOfferBannerDto dto)
    {
        try
        {
            var banner = await service.CreateAsync(dto);
            logger.LogInformation("Offer banner created: {BannerId} Slot={Slot}", banner.BannerId, banner.Slot);
            return Success(banner, "Banner created.");
        }
        catch (ArgumentException ex)
        {
            return Failure(ex.Message);
        }
    }

    /// <summary>PUT /api/admin/offer-banners/{id} — replace banner.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOfferBannerDto dto)
    {
        try
        {
            var banner = await service.UpdateAsync(id, dto);
            logger.LogInformation("Offer banner updated: {BannerId}", banner.BannerId);
            return Success(banner, "Banner updated.");
        }
        catch (KeyNotFoundException ex)
        {
            return Failure(ex.Message, statusCode: 404);
        }
        catch (ArgumentException ex)
        {
            return Failure(ex.Message);
        }
    }

    /// <summary>DELETE /api/admin/offer-banners/{id} — hard delete.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await service.DeleteAsync(id);
            logger.LogInformation("Offer banner deleted: {BannerId}", id);
            return Success(true, "Banner deleted.");
        }
        catch (KeyNotFoundException ex)
        {
            return Failure(ex.Message, statusCode: 404);
        }
    }

    /// <summary>
    /// PATCH /api/admin/offer-banners/{id}/active — toggle IsActive.
    /// Body: { "isActive": true }
    /// </summary>
    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetActiveRequest req)
    {
        try
        {
            var isActive = await service.SetActiveAsync(id, req.IsActive);
            return Success(new { isActive }, isActive ? "Banner activated." : "Banner deactivated.");
        }
        catch (KeyNotFoundException ex)
        {
            return Failure(ex.Message, statusCode: 404);
        }
    }
}
