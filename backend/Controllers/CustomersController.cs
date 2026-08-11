using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Customers;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Email;

namespace MerxoSell.API.Controllers;

/// <summary>SuperAdmin management of buyer accounts.</summary>
[Route("api/admin/customers")]
[Authorize(Roles = "SuperAdmin")]
public class CustomersController : BaseApiController
{
    private readonly ICustomerRepository           _repo;
    private readonly IEmailService                 _email;
    private readonly ILogger<CustomersController>  _logger;

    public CustomersController(ICustomerRepository repo, IEmailService email, ILogger<CustomersController> logger)
    {
        _repo   = repo;
        _email  = email;
        _logger = logger;
    }

    /// <summary>GET /api/admin/customers — list all buyers with optional search and active filter.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? isActive)
    {
        var buyers = await _repo.GetAllBuyersAsync(search, isActive);
        var result = new List<CustomerDto>();

        foreach (var u in buyers)
        {
            var orderCount = await _repo.GetOrderCountAsync(u.UserId);
            var totalSpent = await _repo.GetTotalSpentCadAsync(u.UserId);
            var addrCount  = await _repo.GetAddressCountAsync(u.UserId);
            
            result.Add(new CustomerDto(
                u.UserId, u.FullName, u.Email, u.Phone, u.PreferredCurrency, u.IsActive,
                orderCount, totalSpent, addrCount, u.CreatedAt, u.UpdatedAt
            ));
        }

        return Ok(result);
    }

    /// <summary>GET /api/admin/customers/{id} — full profile, addresses and last-order summary.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var u = await _repo.GetBuyerByIdAsync(id);
        if (u is null) return NotFound(new { error = $"Customer {id} not found." });

        var orderCount  = await _repo.GetOrderCountAsync(id);
        var totalSpent  = await _repo.GetTotalSpentCadAsync(id);
        var addressCnt  = await _repo.GetAddressCountAsync(id);
        var addresses   = await _repo.GetAddressesAsync(id);
        var (lastStatus, lastAt) = await _repo.GetLastOrderAsync(id);

        var addrDtos = addresses.Select(a => new CustomerAddressDto(
            a.AddressId, a.FullName, a.AddressLine1, a.AddressLine2,
            a.City, a.State, a.PostalCode, a.Country, a.Phone, a.IsDefault));

        return Ok(new CustomerDetailDto(
            u.UserId, u.FullName, u.Email, u.Phone, u.PreferredCurrency, u.IsActive,
            orderCount, totalSpent, addressCnt, u.CreatedAt, u.UpdatedAt,
            lastStatus, lastAt, addrDtos));
    }

    /// <summary>PUT /api/admin/customers/{id} — update editable customer fields.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var ok = await _repo.UpdateAsync(new User
        {
            UserId            = id,
            FullName          = dto.FullName.Trim(),
            Phone             = dto.Phone,
            PreferredCurrency = dto.PreferredCurrency,
            IsActive          = dto.IsActive
        });

        if (!ok) return NotFound(new { error = $"Customer {id} not found." });

        _logger.LogInformation("Customer {UserId} updated by admin", id);
        
        // Notify user of status change
        var u = await _repo.GetBuyerByIdAsync(id);
        if (u != null)
            _ = _email.SendAccountStatusNotificationAsync(u.Email, u.FullName, u.IsActive, u.IsDeleted);

        return Ok(new { message = "Customer updated." });
    }

    /// <summary>PATCH /api/admin/customers/{id}/status — activate or deactivate a customer.</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetCustomerStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var ok = await _repo.SetStatusAsync(id, dto.IsActive);
        if (!ok) return NotFound(new { error = $"Customer {id} not found." });

        _logger.LogInformation("Customer {UserId} status set to {IsActive}", id, dto.IsActive);
        
        var u = await _repo.GetBuyerByIdAsync(id);
        if (u != null)
            _ = _email.SendAccountStatusNotificationAsync(u.Email, u.FullName, dto.IsActive, u.IsDeleted);

        return Ok(new { message = $"Customer {(dto.IsActive ? "activated" : "deactivated")}." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.SoftDeleteAsync(id);
        if (!ok) return NotFound(new { error = $"Customer {id} not found." });

        _logger.LogInformation("Customer {UserId} soft-deleted by admin", id);
        
        var u = await _repo.GetBuyerByIdAsync(id);
        if (u != null)
            _ = _email.SendAccountStatusNotificationAsync(u.Email, u.FullName, u.IsActive, true);

        return NoContent();
    }
}
