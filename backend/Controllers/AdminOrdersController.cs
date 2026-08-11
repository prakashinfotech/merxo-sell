using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Admin;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>SuperAdmin management of all marketplace orders (read, status update, soft-cancel).</summary>
[Route("api/admin/orders")]
[Authorize(Roles = "SuperAdmin")]
[ApiController]
public class AdminOrdersController : ControllerBase
{
    private static readonly string[] AllowedStatuses =
    {
        "Pending", "Confirmed", "Processing", "Shipped",
        "Delivered", "Cancelled", "Refunded"
    };

    private readonly AppDbContext                    _db;
    private readonly IOrderService                   _orderService;
    private readonly ILogger<AdminOrdersController>  _logger;

    public AdminOrdersController(AppDbContext db, IOrderService orderService, ILogger<AdminOrdersController> logger)
    {
        _db           = db;
        _orderService = orderService;
        _logger       = logger;
    }

    /// <summary>GET /api/admin/orders — list all orders with optional status/buyer search.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        var q = _db.Orders.AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(o =>
                o.User.FullName.Contains(s) ||
                o.User.Email.Contains(s) ||
                o.OrderId.ToString().Contains(s));
        }

        var orders = await q
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new AdminOrderListDto(
                o.OrderId,
                o.Status,
                o.TotalAmountCAD,
                o.CurrencyCode ?? "CAD",
                o.DisplayTotal ?? o.TotalAmountCAD,
                o.Items.Count,
                o.UserId,
                o.User.FullName,
                o.User.Email,
                o.CreatedAt,
                o.UpdatedAt
            ))
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>GET /api/admin/orders/{id} — full order detail with items and shipping address.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var o = await _db.Orders.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Address)
            .Include(x => x.Items).ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images.Where(img => img.IsPrimary))
            .Include(x => x.Items).ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(x => x.OrderId == id);

        if (o is null) return NotFound(new { error = $"Order #{id} not found." });

        var items = o.Items.Select(i =>
        {
            string? variantInfo = null;
            if (i.Variant is not null)
            {
                var parts = new List<string>();
                if (!string.IsNullOrEmpty(i.Variant.Color)) parts.Add($"Color: {i.Variant.Color}");
                if (!string.IsNullOrEmpty(i.Variant.Size))  parts.Add($"Size: {i.Variant.Size}");
                if (parts.Count > 0) variantInfo = string.Join(", ", parts);
            }

            return new AdminOrderItemDto(
                i.OrderItemId,
                i.ProductId,
                i.VariantId,
                i.Product?.Name ?? "Unknown product",
                i.Product?.Images.FirstOrDefault()?.ImageUrl,
                variantInfo,
                i.Quantity,
                i.UnitPriceCAD);
        });

        AdminOrderShippingAddressDto? addrDto = o.Address is null ? null
            : new AdminOrderShippingAddressDto(
                o.Address.FullName, o.Address.Phone,
                o.Address.AddressLine1, o.Address.AddressLine2,
                o.Address.City, o.Address.State, o.Address.PostalCode, o.Address.Country);

        var dto = new AdminOrderDetailDto(
            o.OrderId, o.Status,
            o.TotalAmountCAD, o.ShippingAmount, o.DiscountAmount,
            o.DisplayTotal ?? o.TotalAmountCAD,
            o.CurrencyCode ?? "CAD",
            o.Notes, o.CreatedAt, o.UpdatedAt,
            o.UserId, o.User.FullName, o.User.Email, o.User.Phone,
            addrDto, items);

        return Ok(dto);
    }

    /// <summary>PATCH /api/admin/orders/{id}/status — update order workflow status.</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!AllowedStatuses.Contains(dto.Status))
            return BadRequest(new { error = $"Status must be one of: {string.Join(", ", AllowedStatuses)}." });

        try 
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            await _orderService.UpdateOrderStatusAsync(
                id, 
                dto.Status, 
                dto.Note, 
                adminId, 
                "Admin"
            );

            return Ok(new { message = $"Order #{id} status set to {dto.Status} and notification sent." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update status for order {OrderId}", id);
            return StatusCode(500, new { error = "Internal server error while updating status." });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        try 
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            await _orderService.CancelOrderAsync(
                id, 
                "Cancelled by Admin", 
                adminId, 
                "Admin"
            );

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel order {OrderId}", id);
            return StatusCode(500, new { error = "Internal server error while cancelling order." });
        }
    }
}
