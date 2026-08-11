using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Common;
using System.Security.Claims;

namespace MerxoSell.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected int CurrentUserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value, out var id) ? id : 0;

    protected string CurrentUserRole =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? string.Empty;

    protected IActionResult Success<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult Failure(string message, IEnumerable<string>? errors = null, int statusCode = 400) =>
        StatusCode(statusCode, ApiResponse<object>.Failure(message, errors));
}
