using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Auth;
using MerxoSell.API.DTOs.Common;

namespace MerxoSell.Tests.Integration;

/// <summary>
/// Replaces SQL Server with an EF Core InMemory store. Each test instance
/// should use a unique DB name so test runs are isolated.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"IntTest_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "YOUR_SECURE_JWT_SECRET_KEY_MIN_32_CHARS_LONG"
            });
        });




        builder.ConfigureTestServices(services =>
        {
            // Remove the real SQL Server DbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName)
                       .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            // Disable real SMTP so tests never try to send email
            services.Configure<MerxoSell.API.Services.Email.SmtpOptions>(o =>
            {
                o.Enabled = false;
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            SeedDatabase(db);
        });
    }

    private static void SeedDatabase(AppDbContext db)
    {
        var superAdminRole = db.Roles.FirstOrDefault(r => r.RoleName == MerxoSell.API.Constants.AppRoles.SuperAdmin)
            ?? new MerxoSell.API.Models.Role { RoleName = MerxoSell.API.Constants.AppRoles.SuperAdmin };
        var sellerRole = db.Roles.FirstOrDefault(r => r.RoleName == MerxoSell.API.Constants.AppRoles.Seller)
            ?? new MerxoSell.API.Models.Role { RoleName = MerxoSell.API.Constants.AppRoles.Seller };
        var buyerRole = db.Roles.FirstOrDefault(r => r.RoleName == MerxoSell.API.Constants.AppRoles.Buyer)
            ?? new MerxoSell.API.Models.Role { RoleName = MerxoSell.API.Constants.AppRoles.Buyer };

        if (!db.Roles.Any())
        {
            db.Roles.AddRange(superAdminRole, sellerRole, buyerRole);
            db.SaveChanges();
        }

        if (!db.CurrencyRates.Any(c => c.CurrencyCode == "CAD"))
        {
            db.CurrencyRates.Add(new MerxoSell.API.Models.CurrencyRate
            {
                CurrencyCode = "CAD",
                CurrencyName = "Canadian Dollar",
                Symbol = "$",
                RateToCad = 1m
            });
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.Email.ToLower() == "admin@merxosell.com"))
        {
            db.Users.Add(new MerxoSell.API.Models.User
            {
                RoleId = superAdminRole.RoleId,
                FullName = "Administrator",
                Email = "Admin@merxosell.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 4),
                PreferredCurrency = "CAD",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        var sellerUser = db.Users.FirstOrDefault(u => u.Email.ToLower() == "seller@merxosell.com");
        if (sellerUser is null)
        {
            sellerUser = new MerxoSell.API.Models.User
            {
                RoleId = sellerRole.RoleId,
                FullName = "Test Seller",
                Email = "seller@merxosell.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Seller@123", workFactor: 4),
                PreferredCurrency = "CAD",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(sellerUser);
            db.SaveChanges();

            db.Sellers.Add(new MerxoSell.API.Models.Seller
            {
                UserId = sellerUser.UserId,
                StoreName = "Test Store",
                IsActive = true
            });
        }

        db.SaveChanges();
    }

}

/// <summary>
/// Shared login helpers used across all integration test classes.
/// </summary>
public static class IntegrationHelpers
{
    public static async Task<string> LoginAdminAsync(HttpClient client)
        => await LoginAsync(client, "Admin@merxosell.com", "Admin@123", "/api/auth/admin/login");

    public static async Task<string> LoginSellerAsync(HttpClient client)
        => await LoginAsync(client, "seller@merxosell.com", "Seller@123", "/api/auth/seller/login");

    /// <summary>Registers a fresh buyer and returns their access token.</summary>
    public static async Task<string> RegisterAndLoginBuyerAsync(HttpClient client, string? suffix = null)
    {
        suffix ??= Guid.NewGuid().ToString("N")[..8];
        var email    = $"buyer_{suffix}@test.com";
        var password = "Buyer@1234!";

        var regDto = new RegisterRequestDto
        {
            FullName        = $"Buyer {suffix}",
            Email           = email,
            Password        = password,
            ConfirmPassword = password,
            Role            = "Buyer"
        };

        var reg = await client.PostAsJsonAsync("/api/auth/register", regDto);
        reg.EnsureSuccessStatusCode();

        return await LoginAsync(client, email, password, "/api/auth/login");
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password, string endpoint)
    {
        var response = await client.PostAsJsonAsync(endpoint, new LoginRequestDto
        {
            Email = email, Password = password
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
        return body!.Data!.AccessToken;
    }
}
