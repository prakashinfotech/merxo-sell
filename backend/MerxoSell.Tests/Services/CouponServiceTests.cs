using FluentAssertions;
using Moq;
using MerxoSell.API.DTOs.Coupons;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services;

namespace MerxoSell.Tests.Services;

/// <summary>
/// CouponService.ValidateAsync is tested against the mock repository;
/// CouponService.CreateAsync / duplicate-code check uses the mock as well.
/// InMemory DbContext is used for GetUsageStatsAsync.
/// </summary>
public class CouponServiceTests : TestBase
{
    private readonly Mock<ICouponRepository> _repo = new();

    private CouponService BuildSut() => new(_repo.Object, Db);

    // ── ValidateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ValidFixedCoupon_ReturnsIsValidWithDiscount()
    {
        var coupon = MakeActiveCoupon(1, "SAVE10", "FixedAmount", 10m);
        _repo.Setup(r => r.GetByCodeAsync("SAVE10")).ReturnsAsync(coupon);
        _repo.Setup(r => r.HasUserUsedCouponAsync(It.IsAny<int>(), It.IsAny<int>()))
             .ReturnsAsync(false);

        var result = await BuildSut().ValidateAsync("SAVE10", 100m, userId: 1);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(10m);
        result.FinalAmount.Should().Be(90m);
    }

    [Fact]
    public async Task Validate_PercentageCoupon_CalculatesDiscount()
    {
        var coupon = MakeActiveCoupon(2, "PCT20", "Percentage", 20m);
        _repo.Setup(r => r.GetByCodeAsync("PCT20")).ReturnsAsync(coupon);
        _repo.Setup(r => r.HasUserUsedCouponAsync(It.IsAny<int>(), It.IsAny<int>()))
             .ReturnsAsync(false);

        var result = await BuildSut().ValidateAsync("PCT20", 50m);

        result.DiscountAmount.Should().Be(10m);
        result.FinalAmount.Should().Be(40m);
    }

    [Fact]
    public async Task Validate_MaximumDiscountCap_LimitsDiscount()
    {
        var coupon = MakeActiveCoupon(3, "CAP5", "Percentage", 50m);
        coupon.MaximumDiscountAmount = 5m;
        _repo.Setup(r => r.GetByCodeAsync("CAP5")).ReturnsAsync(coupon);
        _repo.Setup(r => r.HasUserUsedCouponAsync(It.IsAny<int>(), It.IsAny<int>()))
             .ReturnsAsync(false);

        var result = await BuildSut().ValidateAsync("CAP5", 100m);

        result.DiscountAmount.Should().Be(5m);
    }

    [Fact]
    public async Task Validate_ExpiredCoupon_ReturnsInvalid()
    {
        var coupon = MakeActiveCoupon(4, "OLD", expiry: DateTime.UtcNow.AddDays(-1));
        _repo.Setup(r => r.GetByCodeAsync("OLD")).ReturnsAsync(coupon);

        var result = await BuildSut().ValidateAsync("OLD", 100m);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("expired");
    }

    [Fact]
    public async Task Validate_UsageLimitReached_ReturnsInvalid()
    {
        var coupon = MakeActiveCoupon(5, "LIMIT", usageLimit: 5, usedCount: 5);
        _repo.Setup(r => r.GetByCodeAsync("LIMIT")).ReturnsAsync(coupon);

        var result = await BuildSut().ValidateAsync("LIMIT", 100m);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("usage limit");
    }

    [Fact]
    public async Task Validate_MinimumPurchaseNotMet_ReturnsInvalid()
    {
        var coupon = MakeActiveCoupon(6, "MINBUY", minAmt: 200m);
        _repo.Setup(r => r.GetByCodeAsync("MINBUY")).ReturnsAsync(coupon);

        var result = await BuildSut().ValidateAsync("MINBUY", 50m);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("Minimum purchase");
    }

    [Fact]
    public async Task Validate_UserAlreadyUsedCoupon_ReturnsInvalid()
    {
        var coupon = MakeActiveCoupon(7, "USED");
        _repo.Setup(r => r.GetByCodeAsync("USED")).ReturnsAsync(coupon);
        _repo.Setup(r => r.HasUserUsedCouponAsync(coupon.CouponId, 42)).ReturnsAsync(true);

        var result = await BuildSut().ValidateAsync("USED", 100m, userId: 42);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("already used");
    }

    [Fact]
    public async Task Validate_InactiveCoupon_ReturnsInvalid()
    {
        var coupon = MakeActiveCoupon(8, "INACTIVE");
        coupon.IsActive = false;
        _repo.Setup(r => r.GetByCodeAsync("INACTIVE")).ReturnsAsync(coupon);

        var result = await BuildSut().ValidateAsync("INACTIVE", 100m);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("inactive");
    }

    [Fact]
    public async Task Validate_NotYetStarted_ReturnsInvalid()
    {
        var coupon = MakeActiveCoupon(9, "FUTURE");
        coupon.StartDate = DateTime.UtcNow.AddDays(5);
        _repo.Setup(r => r.GetByCodeAsync("FUTURE")).ReturnsAsync(coupon);

        var result = await BuildSut().ValidateAsync("FUTURE", 100m);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("not active yet");
    }

    [Fact]
    public async Task Validate_CouponNotFound_ReturnsInvalid()
    {
        _repo.Setup(r => r.GetByCodeAsync("GHOST")).ReturnsAsync((Coupon?)null);

        var result = await BuildSut().ValidateAsync("GHOST", 100m);

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_DuplicateCode_ThrowsInvalidOperationException()
    {
        var existing = MakeActiveCoupon(10, "DUP");
        _repo.Setup(r => r.GetByCodeAsync("DUP")).ReturnsAsync(existing);

        var dto = new CreateCouponDto(
            CouponCode: "dup",
            Title: "Dup", Description: null,
            DiscountType: "FixedAmount", DiscountValue: 5m,
            MinimumPurchaseAmount: null, MaximumDiscountAmount: null,
            UsageLimit: null,
            StartDate: DateTime.UtcNow,
            ExpiryDate: null,
            IsActive: true);

        await BuildSut().Invoking(s => s.CreateAsync(dto))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Create_PercentageOver100_ThrowsInvalidOperationException()
    {
        _repo.Setup(r => r.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync((Coupon?)null);

        var dto = new CreateCouponDto(
            CouponCode: "BIGPCT",
            Title: "BigPct", Description: null,
            DiscountType: "Percentage", DiscountValue: 150m,
            MinimumPurchaseAmount: null, MaximumDiscountAmount: null,
            UsageLimit: null,
            StartDate: DateTime.UtcNow,
            ExpiryDate: null,
            IsActive: true);

        await BuildSut().Invoking(s => s.CreateAsync(dto))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*cannot exceed 100*");
    }

    [Fact]
    public async Task Create_ExpiryBeforeStart_ThrowsInvalidOperationException()
    {
        _repo.Setup(r => r.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync((Coupon?)null);

        var dto = new CreateCouponDto(
            CouponCode: "BADEXP",
            Title: "BadExp", Description: null,
            DiscountType: "FixedAmount", DiscountValue: 5m,
            MinimumPurchaseAmount: null, MaximumDiscountAmount: null,
            UsageLimit: null,
            StartDate: DateTime.UtcNow.AddDays(10),
            ExpiryDate: DateTime.UtcNow,
            IsActive: true);

        await BuildSut().Invoking(s => s.CreateAsync(dto))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*Expiry date must be after start*");
    }

    [Fact]
    public async Task Create_ValidCoupon_CallsRepositoryCreateAndReturnsMappedDto()
    {
        _repo.Setup(r => r.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync((Coupon?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<Coupon>()))
             .ReturnsAsync((Coupon c) => { c.CouponId = 99; return c; });

        var dto = new CreateCouponDto(
            CouponCode: "NEW10",
            Title: "New Coupon", Description: null,
            DiscountType: "FixedAmount", DiscountValue: 10m,
            MinimumPurchaseAmount: null, MaximumDiscountAmount: null,
            UsageLimit: 100,
            StartDate: DateTime.UtcNow,
            ExpiryDate: DateTime.UtcNow.AddDays(30),
            IsActive: true);

        var result = await BuildSut().CreateAsync(dto);

        result.CouponCode.Should().Be("NEW10");
        result.DiscountValue.Should().Be(10m);
        _repo.Verify(r => r.CreateAsync(It.IsAny<Coupon>()), Times.Once);
    }
}
