using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using MerxoSell.API.Constants;
using MerxoSell.API.Data;
using MerxoSell.API.Helpers;
using MerxoSell.API.Models;

namespace MerxoSell.Tests;

/// <summary>
/// Shared helpers for every test class: in-memory DbContext, seeded roles/users,
/// and a real JwtHelper wired to predictable test settings.
/// </summary>
public abstract class TestBase : IDisposable
{
    // ── EF InMemory context ───────────────────────────────────────────────────
    protected AppDbContext Db { get; }

    // ── Pre-seeded entities ───────────────────────────────────────────────────
    protected Role BuyerRole  { get; private set; } = null!;
    protected Role SellerRole { get; private set; } = null!;
    protected Role AdminRole  { get; private set; } = null!;

    protected User BuyerUser  { get; private set; } = null!;
    protected User SellerUser { get; private set; } = null!;
    protected User AdminUser  { get; private set; } = null!;
    protected Seller SellerRecord { get; private set; } = null!;

    // ── JWT helper ────────────────────────────────────────────────────────────
    protected JwtHelper Jwt { get; }
    protected IConfiguration JwtConfig { get; }

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        Db = new AppDbContext(options);

        JwtConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]     = "super-secret-test-key-32-chars-ok!!",
                ["Jwt:Issuer"]     = "MerxoSellTest",
                ["Jwt:Audience"]   = "MerxoSellTestAudience",
                ["Jwt:ExpiryMins"] = "60",
            })
            .Build();

        Jwt = new JwtHelper(JwtConfig);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        BuyerRole  = new Role { RoleId = 1, RoleName = AppRoles.Buyer };
        SellerRole = new Role { RoleId = 2, RoleName = AppRoles.Seller };
        AdminRole  = new Role { RoleId = 3, RoleName = AppRoles.SuperAdmin };

        Db.Roles.AddRange(BuyerRole, SellerRole, AdminRole);
        Db.SaveChanges();

        // CAD currency is required because the User entity has a FK to CurrencyRate
        Db.CurrencyRates.Add(new CurrencyRate
        {
            CurrencyCode = "CAD",
            CurrencyName = "Canadian Dollar",
            Symbol       = "$",
            RateToCad    = 1m
        });
        Db.SaveChanges();

        BuyerUser = new User
        {
            UserId       = 10,
            RoleId       = BuyerRole.RoleId,
            Role         = BuyerRole,
            FullName     = "Test Buyer",
            Email        = "buyer@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Buyer@123", workFactor: 4),
            IsActive     = true
        };

        SellerRecord = new Seller
        {
            SellerId  = 5,
            StoreName = "Test Store",
            IsActive  = true
        };

        SellerUser = new User
        {
            UserId       = 11,
            RoleId       = SellerRole.RoleId,
            Role         = SellerRole,
            FullName     = "Test Seller",
            Email        = "seller@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Seller@123", workFactor: 4),
            IsActive     = true,
            Seller       = SellerRecord
        };

        AdminUser = new User
        {
            UserId       = 12,
            RoleId       = AdminRole.RoleId,
            Role         = AdminRole,
            FullName     = "Test Admin",
            Email        = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 4),
            IsActive     = true
        };

        Db.Users.AddRange(BuyerUser, SellerUser, AdminUser);
        Db.Sellers.Add(SellerRecord);
        Db.SaveChanges();

        // Link seller back to user
        SellerRecord.UserId = SellerUser.UserId;
        Db.SaveChanges();
    }

    // ── Factory helpers ───────────────────────────────────────────────────────

    protected static Category MakeCategory(int id, string name, int? parentId = null) => new()
    {
        CategoryId       = id,
        Name             = name,
        Slug             = name.ToLowerInvariant(),
        ParentCategoryId = parentId,
        IsActive         = true
    };

    protected static Coupon MakeActiveCoupon(
        int id,
        string code      = "SAVE10",
        string type      = "FixedAmount",
        decimal value    = 10m,
        int? usageLimit  = null,
        int usedCount    = 0,
        decimal? minAmt  = null,
        DateTime? expiry = null) => new()
    {
        CouponId              = id,
        CouponCode            = code,
        Title                 = code,
        DiscountType          = type,
        DiscountValue         = value,
        UsageLimit            = usageLimit,
        UsedCount             = usedCount,
        MinimumPurchaseAmount = minAmt,
        StartDate             = DateTime.UtcNow.AddDays(-1),
        ExpiryDate            = expiry,
        IsActive              = true
    };

    public void Dispose() => Db.Dispose();
}
