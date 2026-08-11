using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Common;
using MerxoSell.API.DTOs.Products;
using MerxoSell.API.Models;

namespace MerxoSell.Tests.Integration;

/// <summary>
/// End-to-end: Seller creates product (status=Pending) → Admin approves
/// → Buyer can see it in the public catalogue.
/// </summary>
public class SellerApprovalFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SellerApprovalFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient MakeClient(string? token = null)
    {
        var client = _factory.CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ── Seller routes require Seller role ─────────────────────────────────────

    [Fact]
    public async Task SellerProducts_NoToken_Returns401()
    {
        var client   = MakeClient();
        var response = await client.GetAsync("/api/seller/products");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Admin approvals require SuperAdmin role ────────────────────────────────

    [Fact]
    public async Task AdminApprovals_NoToken_Returns401()
    {
        var client   = MakeClient();
        var response = await client.GetAsync("/api/admin/approvals");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminApprovals_BuyerToken_Returns403()
    {
        using var setup = _factory.CreateClient();
        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(setup);
        var client = MakeClient(token);

        var response = await client.GetAsync("/api/admin/approvals");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminApprovals_AdminToken_Returns200()
    {
        using var setup = _factory.CreateClient();
        var token = await IntegrationHelpers.LoginAdminAsync(setup);
        var client = MakeClient(token);

        var response = await client.GetAsync("/api/admin/approvals");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Seller dashboard ───────────────────────────────────────────────────────

    [Fact]
    public async Task SellerDashboard_SellerToken_Returns200()
    {
        using var setup = _factory.CreateClient();
        var token = await IntegrationHelpers.LoginSellerAsync(setup);
        var client = MakeClient(token);

        var response = await client.GetAsync("/api/seller/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Pending product is not visible in public catalogue ────────────────────

    [Fact]
    public async Task PendingProduct_NotVisibleInPublicCatalogue()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var seller = db.Sellers.First();

        var catId = 77;
        if (!db.Categories.Any(c => c.CategoryId == catId))
            db.Categories.Add(new Category { CategoryId = catId, Name = "Approval Test", Slug = "approval-test", IsActive = true });

        const int pendingProductId = 999;
        if (!db.Products.Any(p => p.ProductId == pendingProductId))
        {
            db.Products.Add(new Product
            {
                ProductId  = pendingProductId,
                CategoryId = catId,
                SellerId   = seller.SellerId,
                Name       = "Pending Widget",
                Slug       = "pending-widget",
                BasePrice  = 9.99m,
                Stock      = 10,
                Status     = "Pending",
                IsActive   = true
            });
        }
        db.SaveChanges();

        var client   = MakeClient();
        var response = await client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<ProductListDto>>();

        var pendingInCatalogue = body!.Data.Any(p => p.ProductId == pendingProductId);
        pendingInCatalogue.Should().BeFalse("pending products should not appear in the public catalogue");
    }
}
