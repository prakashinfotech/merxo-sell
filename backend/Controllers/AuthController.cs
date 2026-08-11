using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Auth;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>POST /api/auth/register</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _authService.RegisterAsync(dto);
            return Success(response, "Registration successful.");
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message);
        }
    }

    /// <summary>POST /api/auth/login — regular user login</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _authService.LoginAsync(dto);
            return Success(response, "Login successful.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, statusCode: 401);
        }
    }

    /// <summary>POST /api/auth/admin/login — SuperAdmin-only login</summary>
    [HttpPost("admin/login")]
    public async Task<IActionResult> AdminLogin([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _authService.AdminLoginAsync(dto);
            return Success(response, "Admin login successful.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, statusCode: 401);
        }
    }

    /// <summary>POST /api/auth/seller/login — Seller-only login</summary>
    [HttpPost("seller/login")]
    public async Task<IActionResult> SellerLogin([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _authService.SellerLoginAsync(dto);
            return Success(response, "Seller login successful.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Failure(ex.Message, statusCode: 401);
        }
    }
}
