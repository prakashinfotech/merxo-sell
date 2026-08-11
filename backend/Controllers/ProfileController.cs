using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MerxoSell.API.DTOs.Profile;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

[Route("api/profile")]
[Authorize]
public class ProfileController : BaseApiController
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));

    /// <summary>GET /api/profile</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var profile = await _profileService.GetProfileAsync(GetUserId());
            return Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "User not found." });
        }
    }

    /// <summary>PUT /api/profile</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var profile = await _profileService.UpdateProfileAsync(GetUserId(), dto);
            return Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "User not found." });
        }
    }

    /// <summary>GET /api/profile/addresses — list addresses for the current user.</summary>
    [HttpGet("addresses")]
    public async Task<IActionResult> GetAddresses()
    {
        var addresses = await _profileService.GetAddressesAsync(GetUserId());
        return Ok(addresses);
    }

    /// <summary>POST /api/profile/addresses</summary>
    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var address = await _profileService.AddAddressAsync(GetUserId(), dto);
            return StatusCode(201, address);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>PUT /api/profile/addresses/{id}</summary>
    [HttpPut("addresses/{id:int}")]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateAddressDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var address = await _profileService.UpdateAddressAsync(GetUserId(), id, dto);
            return Ok(address);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Address not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>DELETE /api/profile/addresses/{id}</summary>
    [HttpDelete("addresses/{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        try
        {
            await _profileService.DeleteAddressAsync(GetUserId(), id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Address not found." });
        }
    }

    [HttpPatch("addresses/{id:int}/default")]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        try
        {
            var address = await _profileService.SetDefaultAddressAsync(GetUserId(), id);
            return Ok(address);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Address not found." });
        }
    }

    /// <summary>POST /api/profile/addresses/{id}/set-default — alias for setting the default address.</summary>
    [HttpPost("addresses/{id:int}/set-default")]
    public Task<IActionResult> SetDefaultAddressAlias(int id) => SetDefaultAddress(id);

    /// <summary>POST /api/profile/payments</summary>
    [HttpPost("payments")]
    public async Task<IActionResult> AddPaymentMethod([FromBody] CreatePaymentMethodDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var pm = await _profileService.AddPaymentMethodAsync(GetUserId(), dto);
        return StatusCode(201, pm);
    }

    /// <summary>DELETE /api/profile/payments/{id}</summary>
    [HttpDelete("payments/{id:int}")]
    public async Task<IActionResult> DeletePaymentMethod(int id)
    {
        try
        {
            await _profileService.DeletePaymentMethodAsync(GetUserId(), id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Payment method not found." });
        }
    }

    /// <summary>PATCH /api/profile/payments/{id}/default</summary>
    [HttpPatch("payments/{id:int}/default")]
    public async Task<IActionResult> SetDefaultPaymentMethod(int id)
    {
        try
        {
            var pm = await _profileService.SetDefaultPaymentMethodAsync(GetUserId(), id);
            return Ok(pm);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Payment method not found." });
        }
    }

    /// <summary>POST /api/profile/change-password</summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _profileService.ChangePasswordAsync(GetUserId(), dto.CurrentPassword, dto.NewPassword);
            return Ok(new { message = "Password changed successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>GET /api/profile/seller-store</summary>
    [HttpGet("seller-store")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerStore()
    {
        var store = await _profileService.GetSellerStoreAsync(GetUserId());
        return store is null ? NotFound(new { error = "Seller profile not found." }) : Ok(store);
    }

    /// <summary>PUT /api/profile/seller-store</summary>
    [HttpPut("seller-store")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UpdateSellerStore([FromBody] MerxoSell.API.DTOs.Seller.UpdateSellerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _profileService.UpdateSellerStoreAsync(GetUserId(), dto);
        return Ok(new { message = "Store settings updated successfully." });
    }
}
