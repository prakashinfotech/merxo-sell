using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MerxoSell.API.Constants;
using MerxoSell.API.Models;

namespace MerxoSell.API.Helpers;

public class JwtHelper
{
    private readonly IConfiguration _config;

    public JwtHelper(IConfiguration config) => _config = config;

    /// <summary>
    /// Generates a signed JWT including: sub, email, role, sellerId (Seller role only),
    /// preferredCurrency, and iat claims.
    /// </summary>
    public string GenerateToken(User user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
        var creds       = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier,     user.UserId.ToString()),
            new(ClaimTypes.Role,               user.Role.RoleName),
            new("role",                        user.Role.RoleName),
            new("preferredCurrency",           user.PreferredCurrency),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        // Include sellerId claim only for Seller role — used by SellerProductsController
        // to scope all queries to the authenticated seller's records.
        if (user.Role.RoleName == AppRoles.Seller && user.Seller is not null)
            claims.Add(new Claim("sellerId", user.Seller.SellerId.ToString()));

        var expiry = DateTime.UtcNow.AddMinutes(
            int.Parse(jwtSettings["ExpiryMins"] ?? "1440"));  // default 24 hours

        var token = new JwtSecurityToken(
            issuer:             jwtSettings["Issuer"],
            audience:           jwtSettings["Audience"],
            claims:             claims,
            expires:            expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
