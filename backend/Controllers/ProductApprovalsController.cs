using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MerxoSell.API.DTOs.Approval;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>Super Admin: review, approve, and reject pending product listings.</summary>
[Route("api/admin/approvals")]
[Authorize(Roles = "SuperAdmin")]
public class ProductApprovalsController : BaseApiController
{
    private readonly IProductApprovalRepository _approvalRepo;
    private readonly ILogger<ProductApprovalsController> _logger;

    public ProductApprovalsController(
        IProductApprovalRepository approvalRepo,
        ILogger<ProductApprovalsController> logger)
    {
        _approvalRepo = approvalRepo;
        _logger       = logger;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));

    /// <summary>GET /api/admin/approvals — pending queue ordered oldest first.</summary>
    [HttpGet]
    public async Task<IActionResult> GetQueue()
    {
        var items = await _approvalRepo.GetPendingQueueAsync();
        var result = items.Select(p => new ApprovalQueueItemDto(
            p.ProductId, p.Name, p.Slug, p.BasePrice, p.Status,
            p.Seller.StoreName, p.Seller.User.Email,
            p.Category.Name,
            p.Images.FirstOrDefault()?.ImageUrl,
            p.CreatedAt));
        return Ok(result);
    }

    /// <summary>GET /api/admin/approvals/history — paginated approval log.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int? productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1)     page     = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var logs = await _approvalRepo.GetHistoryAsync(productId, page, pageSize);
        var result = logs.Select(l => new ApprovalLogDto(
            l.LogId, l.ProductId, l.Product.Name,
            l.Reviewer?.Email,
            l.OldStatus, l.NewStatus, l.Note, l.CreatedAt));
        return Ok(result);
    }

    /// <summary>POST /api/admin/approvals/{id}/approve — approve a pending product.</summary>
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveProductDto dto)
    {
        var userId = GetUserId();
        try
        {
            await _approvalRepo.ApproveAsync(id, userId, dto.Note);
            _logger.LogInformation("Product {ProductId} approved by user {UserId}", id, userId);
            return Ok(new { message = "Product approved and is now live.", productId = id });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>POST /api/admin/approvals/{id}/reject — reject with required note.</summary>
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        try
        {
            await _approvalRepo.RejectAsync(id, userId, dto.Note);
            _logger.LogInformation("Product {ProductId} rejected by user {UserId} — Note: {Note}",
                id, userId, dto.Note);
            return Ok(new { message = "Product rejected. Seller has been notified.", productId = id });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
