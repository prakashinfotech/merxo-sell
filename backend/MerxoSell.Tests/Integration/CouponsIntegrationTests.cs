using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Coupons;
using MerxoSell.API.Models;

namespace MerxoSell.Tests.Integration;

public class CouponsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CouponsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private void SeedCoupon(string code, bool isActive = true, DateTime? expiry = null,
        decimal? minAmt = null, int? usageLimit = null, int usedCount = 0)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Coupons.Any(c => c.CouponCode == code)) return;

        db.Coupons.Add(new Coupon
        {
            CouponCode            = code,
            Title                 = code,
            DiscountType          = CouponDiscountTypes.FixedAmount,
            DiscountValue         = 10m,
            IsActive              = isActive,
            StartDate             = DateTime.UtcNow.AddDays(-1),
            ExpiryDate            = expiry,
            MinimumPurchaseAmount = minAmt,
            UsageLimit            = usageLimit,
            UsedCount             = usedCount
        });
        db.SaveChanges();
    }

    // ── GET /api/coupons/available ─────────────────────────────────────────────

    [Fact]
    public async Task GetAvailable_NoAuth_Returns200()
    {
        var response = await _client.GetAsync("/api/coupons/available");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── POST /api/coupons/validate ─────────────────────────────────────────────

    [Fact]
    public async Task Validate_NoAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/coupons/validate",
            new ApplyCouponDto("SAVE10", 100m));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_ValidCoupon_BuyerToken_Returns200()
    {
        SeedCoupon("INTTEST10");

        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client);
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/coupons/validate",
            new ApplyCouponDto("INTTEST10", 100m));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CouponValidationResultDto>();
        body!.IsValid.Should().BeTrue();
        body.DiscountAmount.Should().Be(10m);
    }

    [Fact]
    public async Task Validate_ExpiredCoupon_Returns400()
    {
        SeedCoupon("EXPIRED_INT", expiry: DateTime.UtcNow.AddDays(-2));

        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client);
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/coupons/validate",
            new ApplyCouponDto("EXPIRED_INT", 100m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<CouponValidationResultDto>();
        body!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_MinOrderNotMet_Returns400()
    {
        SeedCoupon("MINORD_INT", minAmt: 500m);

        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client);
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/coupons/validate",
            new ApplyCouponDto("MINORD_INT", 50m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Validate_InactiveCoupon_Returns400()
    {
        SeedCoupon("INACTIVE_INT", isActive: false);

        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client);
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/coupons/validate",
            new ApplyCouponDto("INACTIVE_INT", 100m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Admin coupon management ────────────────────────────────────────────────

    [Fact]
    public async Task AdminCoupons_NoAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/admin/coupons");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminCoupons_BuyerToken_Returns403()
    {
        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client);
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/admin/coupons");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
