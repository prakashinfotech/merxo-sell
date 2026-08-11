using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Common;
using MerxoSell.API.DTOs.Orders;
using MerxoSell.API.DTOs.Profile;
using MerxoSell.API.Models;

namespace MerxoSell.Tests.Integration;

public class OrdersIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OrdersIntegrationTests(CustomWebApplicationFactory factory)
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

    private Product SeedProduct(AppDbContext db)
    {
        if (db.Products.Any(p => p.ProductId == 500)) return db.Products.Find(500)!;

        var catId = 99;
        if (!db.Categories.Any(c => c.CategoryId == catId))
            db.Categories.Add(new Category { CategoryId = catId, Name = "Order Test", Slug = "order-test", IsActive = true });

        var sellerUser = db.Users.First(u => u.Email == "seller@merxosell.com");
        var seller = db.Sellers.FirstOrDefault(s => s.UserId == sellerUser.UserId);
        if (seller is null)
        {
            seller = new Seller { UserId = sellerUser.UserId, StoreName = "Order Store", IsActive = true };
            db.Sellers.Add(seller);
            db.SaveChanges();
        }

        var product = new Product
        {
            ProductId  = 500,
            CategoryId = catId,
            SellerId   = seller.SellerId,
            Name       = "Order Test Product",
            Slug       = "order-test-product",
            BasePrice  = 25m,
            Stock      = 100,
            Status     = "Approved",
            IsActive   = true
        };
        db.Products.Add(product);
        db.SaveChanges();
        return product;
    }

    private async Task<(string Token, int AddressId)> SetupBuyerWithAddressAsync()
    {
        using var setupClient = _factory.CreateClient();
        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(setupClient);

        var addrClient = MakeClient(token);
        var addrResp = await addrClient.PostAsJsonAsync("/api/profile/addresses",
            new CreateAddressDto("Test Buyer", "1 Main St", null, "Toronto", "ON", "M1A 1A1", "Canada", null, null));
        addrResp.EnsureSuccessStatusCode();
        var addr = await addrResp.Content.ReadFromJsonAsync<AddressDto>();

        return (token, addr!.AddressId);
    }

    // ── Auth guards ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyOrders_NoToken_Returns401()
    {
        var client   = MakeClient();
        var response = await client.GetAsync("/api/orders");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PlaceOrder_NoToken_Returns401()
    {
        var client   = MakeClient();
        var response = await client.PostAsJsonAsync("/api/orders",
            new CreateOrderDto(1, "CAD", 50m, 0m, 0m, null));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy-path place order ────────────────────────────────────────────────

    [Fact]
    public async Task PlaceOrder_BuyerWithStock_Returns200AndDecrementsStock()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var product = SeedProduct(db);
        var initialStock = product.Stock;

        var (token, addressId) = await SetupBuyerWithAddressAsync();
        var client = MakeClient(token);

        var dto = new CreateOrderDto(
            AddressId:      addressId,
            CurrencyCode:   "CAD",
            DisplayTotal:   50m,
            ShippingAmount: 0m,
            DiscountAmount: 0m,
            CouponCode:     null,
            Items:          new List<CreateOrderItemDto>
            {
                new(ProductId: product.ProductId, VariantId: null, Quantity: 2)
            });

        var response = await client.PostAsJsonAsync("/api/orders", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<OrderDto>();
        body!.Status.Should().Be("Pending");

        // Verify stock was decremented in the DB
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb  = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var refreshed = await verifyDb.Products.FindAsync(product.ProductId);
        refreshed!.Stock.Should().Be(initialStock - 2);
    }

    // ── Get buyer's own orders ────────────────────────────────────────────────

    [Fact]
    public async Task GetMyOrders_AuthenticatedBuyer_Returns200()
    {
        using var setupClient = _factory.CreateClient();
        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(setupClient);

        var client   = MakeClient(token);
        var response = await client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>();
        body.Should().NotBeNull();
    }

    // ── Admin order management ────────────────────────────────────────────────

    [Fact]
    public async Task AdminOrders_NoToken_Returns401()
    {
        var client   = MakeClient();
        var response = await client.GetAsync("/api/admin/orders");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminOrders_AdminToken_Returns200()
    {
        using var setupClient = _factory.CreateClient();
        var token = await IntegrationHelpers.LoginAdminAsync(setupClient);

        var client   = MakeClient(token);
        var response = await client.GetAsync("/api/admin/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminOrders_BuyerToken_Returns403()
    {
        using var setupClient = _factory.CreateClient();
        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(setupClient);

        var client   = MakeClient(token);
        var response = await client.GetAsync("/api/admin/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
