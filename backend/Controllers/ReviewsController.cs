using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MerxoSell.API.DTOs.Reviews;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

[Route("api/reviews")]
public class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));

    private int GetSellerId() =>
        int.Parse(User.FindFirstValue("sellerId")
            ?? throw new UnauthorizedAccessException("Seller identity not found."));

    /// <summary>POST /api/reviews</summary>
    [HttpPost]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var review = await _reviewService.AddReviewAsync(GetUserId(), dto);
            return StatusCode(201, review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>GET /api/reviews/product/{productId}</summary>
    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId);
        return Ok(reviews);
    }

    /// <summary>GET /api/reviews/me — reviews written by the current buyer.</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> GetMyReviews()
    {
        var reviews = await _reviewService.GetMyReviewsAsync(GetUserId());
        return Ok(reviews);
    }

    /// <summary>GET /api/reviews/admin — SuperAdmin review management list.</summary>
    [HttpGet("admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAdminReviews([FromQuery] string? search, [FromQuery] int? rating)
    {
        var reviews = await _reviewService.GetPortalReviewsAsync(search, rating);
        return Ok(reviews);
    }

    /// <summary>GET /api/reviews/seller — Seller-scoped review management list.</summary>
    [HttpGet("seller")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerReviews([FromQuery] string? search, [FromQuery] int? rating)
    {
        var reviews = await _reviewService.GetSellerReviewsAsync(GetSellerId(), search, rating);
        return Ok(reviews);
    }
}
