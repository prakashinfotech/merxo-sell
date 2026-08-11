using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;

namespace MerxoSell.API.Middleware;

/// <summary>
/// Verifies that the authenticated user is still Active and not Deleted.
/// If they have been deactivated/deleted by an admin, this middleware 
/// immediately returns 401 Unauthorized, effectively "logging them out" 
/// from the API perspective even if their JWT is still valid.
/// </summary>
public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = context.User.FindFirst("sub")?.Value;
            if (int.TryParse(userIdStr, out var userId))
            {
                // Check if user is active and not deleted
                var user = await db.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == userId)
                    .Select(u => new { u.IsActive, u.IsDeleted })
                    .FirstOrDefaultAsync();

                if (user == null || !user.IsActive || user.IsDeleted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { 
                        error = "Unauthorized", 
                        message = "Your account has been deactivated or deleted. Please contact support." 
                    });
                    return;
                }
            }
        }

        await _next(context);
    }
}
