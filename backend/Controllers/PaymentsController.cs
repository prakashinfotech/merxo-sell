using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Payments;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

[Route("api/payments")]
[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IRazorpayService _razorpayService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IRazorpayService razorpayService, ILogger<PaymentsController> logger)
    {
        _razorpayService = razorpayService;
        _logger = logger;
    }

    /// <summary>GET /api/payments/razorpay/config — Public configuration info (KeyId, Email).</summary>
    [HttpGet("razorpay/config")]
    [AllowAnonymous]
    public IActionResult GetRazorpayConfig()
    {
        var config = _razorpayService.GetConfig();
        return Ok(config);
    }

    /// <summary>POST /api/payments/razorpay/create-order — Creates Razorpay order for checkout.</summary>
    [HttpPost("razorpay/create-order")]
    public async Task<IActionResult> CreateRazorpayOrder([FromBody] CreateRazorpayOrderRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _razorpayService.CreateOrderAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Razorpay order");
            return StatusCode(500, new { error = "Failed to create Razorpay payment order." });
        }
    }

    /// <summary>POST /api/payments/razorpay/verify — Verifies Razorpay payment signature after checkout.</summary>
    [HttpPost("razorpay/verify")]
    public async Task<IActionResult> VerifyRazorpayPayment([FromBody] VerifyRazorpayPaymentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            bool isValid = await _razorpayService.VerifyPaymentAsync(dto);
            if (!isValid)
            {
                return BadRequest(new { error = "Invalid Razorpay payment signature." });
            }

            return Ok(new { success = true, message = "Razorpay payment verified successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Razorpay payment");
            return StatusCode(500, new { error = "Payment verification failed." });
        }
    }
}
