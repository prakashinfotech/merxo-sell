using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MerxoSell.API.DTOs.Orders;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

[Route("api/orders")]
[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService            _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger       = logger;
    }

    private int    GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));
    private string GetRole() => User.FindFirstValue(ClaimTypes.Role) ?? "Buyer";

    /// <summary>POST /api/orders — Buyer places a new order from their cart.</summary>
    [HttpPost]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var order = await _orderService.CreateOrderAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>GET /api/orders — Caller's own orders. Always returns 200.</summary>
    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        try
        {
            var orders = await _orderService.GetUserOrdersAsync(GetUserId());
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch orders for user {UserId}", GetUserId());
            return StatusCode(500, new { error = "Could not load your orders. Please try again." });
        }
    }

    /// <summary>GET /api/orders/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        try
        {
            int? userId = User.IsInRole("Buyer") ? GetUserId() : null;
            var order = await _orderService.GetOrderAsync(id, userId);
            return order is null
                ? NotFound(new { error = $"Order #{id} not found." })
                : Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch order {OrderId}", id);
            return StatusCode(500, new { error = "Could not load this order." });
        }
    }

    /// <summary>GET /api/orders/{id}/history — Status timeline.</summary>
    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetOrderHistory(int id)
    {
        var rows = await _orderService.GetStatusHistoryAsync(id);
        return Ok(rows);
    }

    /// <summary>POST /api/orders/{id}/cancel — Buyer-driven cancellation with reason.</summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _orderService.CancelOrderAsync(id, dto.CancellationReason, GetUserId(), GetRole());
            return Ok(new { message = "Order cancelled." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>GET /api/orders/all — Admin/SuperAdmin only.</summary>
    [HttpGet("all")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>PATCH /api/orders/{id}/status — Admin/SuperAdmin transition.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _orderService.UpdateOrderStatusAsync(id, dto.Status, dto.Note, GetUserId(), GetRole());
            return Ok(new { message = $"Order #{id} updated to {dto.Status}." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
