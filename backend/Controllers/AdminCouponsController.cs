using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Coupons;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

[Route("api/admin/coupons")]
[Authorize(Roles = "SuperAdmin")]
public class AdminCouponsController : BaseApiController
{
    private readonly ICouponService _couponService;

    public AdminCouponsController(ICouponService couponService) => _couponService = couponService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CouponDto>>> GetAll()
        => Ok(await _couponService.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<CouponDto>> Create([FromBody] CreateCouponDto dto)
    {
        try
        {
            var coupon = await _couponService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id = coupon.CouponId }, coupon);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CouponDto>> Update(int id, [FromBody] UpdateCouponDto dto)
    {
        try { return Ok(await _couponService.UpdateAsync(id, dto)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPatch("{id:int}/active")]
    public async Task<ActionResult<CouponDto>> SetActive(int id, [FromBody] SetCouponActiveDto dto)
    {
        try { return Ok(await _couponService.SetActiveAsync(id, dto.IsActive)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _couponService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<IReadOnlyList<CouponUsageStatDto>>> GetStats()
        => Ok(await _couponService.GetUsageStatsAsync());
}

public record SetCouponActiveDto(bool IsActive);
