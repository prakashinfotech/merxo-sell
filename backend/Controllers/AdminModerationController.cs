using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MerxoSell.API.DTOs.Admin;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>
/// Admin moderation routes (spec-aligned). Product approval queue + review moderation.
/// All actions require the SuperAdminOnly policy.
/// </summary>
[Route("api/admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class AdminModerationController : BaseApiController
{
    private readonly IProductApprovalRepository _approvalRepo;
    private readonly IReviewRepository          _reviewRepo;
    private readonly ILogger<AdminModerationController> _logger;

    public AdminModerationController(
        IProductApprovalRepository approvalRepo,
        IReviewRepository reviewRepo,
        ILogger<AdminModerationController> logger)
    {
        _approvalRepo = approvalRepo;
        _reviewRepo   = reviewRepo;
        _logger       = logger;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));

    // ── Product moderation ────────────────────────────────────────────────

    /// <summary>GET /api/admin/products/pending — products awaiting approval.</summary>
    [HttpGet("products/pending")]
    public async Task<ActionResult<IEnumerable<PendingProductDto>>> GetPendingProducts()
    {
        var products = await _approvalRepo.GetPendingQueueAsync();
        var result = products.Select(p => new PendingProductDto(
            p.ProductId, p.Name, p.Slug, p.BasePrice, p.SalePrice, p.Stock, p.Status,
            p.Seller.StoreName, p.Seller.User.Email, p.Category.Name,
            p.Images.FirstOrDefault()?.ImageUrl, p.CreatedAt));
        return Ok(result);
    }

    /// <summary>POST /api/admin/products/{id}/approve — approve a pending product.</summary>
    [HttpPost("products/{id:int}/approve")]
    public async Task<IActionResult> ApproveProduct(int id, [FromBody] ApproveDto? dto)
    {
        try
        {
            await _approvalRepo.ApproveAsync(id, GetUserId(), dto?.Note);
            _logger.LogInformation("Moderation: product {ProductId} approved by {UserId}", id, GetUserId());
            return Ok(new { message = "Product approved.", productId = id });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>POST /api/admin/products/{id}/reject — reject a pending product with required reason.</summary>
    [HttpPost("products/{id:int}/reject")]
    public async Task<IActionResult> RejectProduct(int id, [FromBody] RejectDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _approvalRepo.RejectAsync(id, GetUserId(), dto.Reason);
            _logger.LogInformation("Moderation: product {ProductId} rejected by {UserId}", id, GetUserId());
            return Ok(new { message = "Product rejected.", productId = id });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ── Review moderation ─────────────────────────────────────────────────

    /// <summary>GET /api/admin/reviews/flagged — reviews flagged for moderator attention.</summary>
    [HttpGet("reviews/flagged")]
    public async Task<ActionResult<IEnumerable<FlaggedReviewDto>>> GetFlaggedReviews()
    {
        var reviews = await _reviewRepo.GetFlaggedAsync();
        var result = reviews.Select(r => new FlaggedReviewDto(
            r.ReviewId,
            r.ProductId,
            r.Product.Name,
            r.Product.Images.FirstOrDefault()?.ImageUrl,
            r.UserId,
            r.User.FullName,
            r.User.Email,
            r.Rating,
            r.Comment,
            r.FlagReason,
            r.CreatedAt));
        return Ok(result);
    }

    /// <summary>POST /api/admin/reviews/{id}/approve — clear the flag and republish.</summary>
    [HttpPost("reviews/{id:int}/approve")]
    public async Task<IActionResult> ApproveReview(int id)
    {
        var review = await _reviewRepo.GetByIdForModerationAsync(id);
        if (review is null) return NotFound(new { error = $"Review {id} not found." });

        review.Status      = "Approved";
        review.FlagReason  = null;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedBy = GetUserId();
        await _reviewRepo.UpdateModerationStatusAsync(review);

        _logger.LogInformation("Moderation: review {ReviewId} approved by {UserId}", id, GetUserId());
        return Ok(new { message = "Review approved.", reviewId = id });
    }

    /// <summary>POST /api/admin/reviews/{id}/reject — reject and hide the review.</summary>
    [HttpPost("reviews/{id:int}/reject")]
    public async Task<IActionResult> RejectReview(int id, [FromBody] RejectDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var review = await _reviewRepo.GetByIdForModerationAsync(id);
        if (review is null) return NotFound(new { error = $"Review {id} not found." });

        review.Status      = "Rejected";
        review.FlagReason  = dto.Reason;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedBy = GetUserId();
        await _reviewRepo.UpdateModerationStatusAsync(review);

        _logger.LogInformation("Moderation: review {ReviewId} rejected by {UserId}", id, GetUserId());
        return Ok(new { message = "Review rejected.", reviewId = id });
    }

    /// <summary>POST /api/admin/reviews/{id}/flag — flag a review for follow-up moderation.</summary>
    [HttpPost("reviews/{id:int}/flag")]
    public async Task<IActionResult> FlagReview(int id, [FromBody] FlagReviewDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var review = await _reviewRepo.GetByIdForModerationAsync(id);
        if (review is null) return NotFound(new { error = $"Review {id} not found." });

        review.Status      = "Flagged";
        review.FlagReason  = dto.Reason;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedBy = GetUserId();
        await _reviewRepo.UpdateModerationStatusAsync(review);

        return Ok(new { message = "Review flagged.", reviewId = id });
    }
}
