using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MerxoSell.API.DTOs.Coupons;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

[Route("api/coupons")]
public class CouponsController : BaseApiController
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService) => _couponService = couponService;

    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<CouponSummaryDto>>> GetAvailable()
        => Ok(await _couponService.GetAvailableAsync());

    [HttpPost("validate")]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<CouponValidationResultDto>> Validate([FromBody] ApplyCouponDto dto)
    {
        var result = await _couponService.ValidateAsync(dto.CouponCode, dto.OrderAmount, GetUserId());
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    [HttpPost("apply")]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<CouponValidationResultDto>> Apply([FromBody] ApplyCouponDto dto)
    {
        var result = await _couponService.ValidateAsync(dto.CouponCode, dto.OrderAmount, GetUserId());
        return result.IsValid ? Ok(result) : BadRequest(result);
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));
}
