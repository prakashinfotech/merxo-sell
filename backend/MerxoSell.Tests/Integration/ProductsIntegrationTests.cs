using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Common;
using MerxoSell.API.DTOs.Products;
using MerxoSell.API.DTOs.Seller;
using MerxoSell.API.Models;

namespace MerxoSell.Tests.Integration;

public class ProductsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private void SeedProduct()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Products.Any(p => p.ProductId == 1)) return;

        if (!db.Categories.Any(c => c.CategoryId == 1))
        {
            db.Categories.Add(new Category
            {
                CategoryId = 1, Name = "Electronics", Slug = "electronics", IsActive = true
            });
        }

        var seller = db.Sellers.FirstOrDefault();
        if (seller is null)
        {
            var sellerUser = db.Users.First(u => u.Email == "seller@merxosell.com");
            seller = new Seller { UserId = sellerUser.UserId, StoreName = "Test Store", IsActive = true };
            db.Sellers.Add(seller);
            db.SaveChanges();
        }

        db.Products.Add(new Product
        {
            ProductId  = 1,
            CategoryId = 1,
            SellerId   = seller.SellerId,
            Name       = "Test Laptop",
            Slug       = "test-laptop",
            BasePrice  = 999.99m,
            Stock      = 50,
            Status     = "Approved",
            IsActive   = true
        });

        db.SaveChanges();
    }

    // ── GET /api/products ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_NoAuth_Returns200()
    {
        var response = await _client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProducts_ReturnsPagedResult()
    {
        SeedProduct();

        var response = await _client.GetAsync("/api/products?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<ProductListDto>>();
        body!.Pagination.Should().NotBeNull();
        body.Pagination.Page.Should().Be(1);
    }

    // ── GET /api/products/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task GetProduct_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync("/api/products/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/products/search ──────────────────────────────────────────────

    [Fact]
    public async Task SearchProducts_EmptyQuery_Returns200()
    {
        var response = await _client.GetAsync("/api/products/search?q=test");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchProducts_Suggest_ShortQuery_ReturnsEmpty()
    {
        var response = await _client.GetAsync("/api/products/suggest?q=a");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<SuggestItemDto>>();
        body!.Should().BeEmpty();
    }

    // ── GET /api/products — requires auth on seller routes ───────────────────

    [Fact]
    public async Task SellerProducts_NoToken_Returns401()
    {
        var response = await _client.GetAsync("/api/seller/products");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SellerProducts_BuyerToken_Returns403()
    {
        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/seller/products");
        _client.DefaultRequestHeaders.Authorization = null;

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateProduct_FashionProductWithVariants_VariantImagesPreserved()
    {
        // 1. Prepare: Log in as Seller
        var token = await IntegrationHelpers.LoginSellerAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Prepare: Create a Category (must be isFashion = true or we just mock a category)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cat = db.Categories.FirstOrDefault(c => c.CategoryId == 10);
            if (cat == null)
            {
                db.Categories.Add(new Category { CategoryId = 10, Name = "Fashion Apparel", Slug = "fashion-apparel", IsActive = true, IsFashion = true });
                db.SaveChanges();
            }
        }

        // 3. Create Product via POST /api/seller/products
        var createDto = new CreateSellerProductDto(
            Name: "Fashion Dress",
            CategoryId: 10,
            Description: "A beautiful dress",
            BasePrice: 49.99m,
            SalePrice: null,
            Stock: 100,
            ManufacturerId: null,
            Images: new List<CreateProductImageDto>
            {
                new CreateProductImageDto("http://example.com/primary.jpg", true),
                new CreateProductImageDto("http://example.com/sec1.jpg", false),
                new CreateProductImageDto("http://example.com/var-red.jpg", false) // variant-to-be image
            },
            Variants: null,
            Colors: new List<string> { "Red" },
            Sizes: new List<string> { "M" }
        );

        var createResponse = await _client.PostAsJsonAsync("/api/seller/products", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createResult = await createResponse.Content.ReadFromJsonAsync<MerxoSell.API.DTOs.Seller.SellerProductListDto>();
        var productId = createResult!.ProductId;

        // 4. Create variant with variant-specific image URL
        var upsertVariantsDto = new MerxoSell.API.DTOs.Seller.BulkUpsertVariantsDto(
            Variants: new List<MerxoSell.API.DTOs.Seller.UpsertVariantWithIdDto>
            {
                new MerxoSell.API.DTOs.Seller.UpsertVariantWithIdDto(
                    VariantId: null,
                    Color: "Red",
                    Size: "M",
                    SKU: "RED-M-SKU",
                    PriceDelta: 0m,
                    Stock: 50,
                    IsActive: true,
                    IsDefault: true,
                    ImageUrls: new List<string> { "http://example.com/var-red.jpg" }
                )
            }
        );

        var variantResponse = await _client.PutAsJsonAsync($"/api/seller/products/{productId}/variants", upsertVariantsDto);
        variantResponse.EnsureSuccessStatusCode();

        // Verify variant and image are stored & linked
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var p = db.Products
                .Include(prod => prod.Variants)
                    .ThenInclude(v => v.Images)
                .First(prod => prod.ProductId == productId);
            p.Variants.Should().HaveCount(1);
            p.Variants.First().Images.Should().HaveCount(1);
            p.Variants.First().Images.First().ImageUrl.Should().Be("http://example.com/var-red.jpg");
        }

        // 5. Update Product details (like changing description) via PUT /api/seller/products/{id}
        // mimicking frontend: primary image + main secondary images (excluding variant images)
        var updateDto = new UpdateSellerProductDto(
            Name: "Fashion Dress Updated",
            CategoryId: 10,
            Description: "An even more beautiful dress",
            BasePrice: 49.99m,
            SalePrice: null,
            Stock: 100,
            ManufacturerId: null,
            Images: new List<CreateProductImageDto>
            {
                new CreateProductImageDto("http://example.com/primary.jpg", true),
                new CreateProductImageDto("http://example.com/sec1.jpg", false)
            },
            Colors: new List<string> { "Red" },
            Sizes: new List<string> { "M" }
        );

        var updateResponse = await _client.PutAsJsonAsync($"/api/seller/products/{productId}", updateDto);
        updateResponse.EnsureSuccessStatusCode();

        // Intermediate check: did UpdateProduct delete the variant images?
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var p = db.Products
                .Include(prod => prod.Variants)
                    .ThenInclude(v => v.Images)
                .First(prod => prod.ProductId == productId);
            p.Variants.First().Images.Should().HaveCount(1, "variant images should survive UpdateProduct call!");
        }

        // 6. Mimic frontend: persist variants next (which Angular form does)
        // Load the variant ID first
        int? createdVariantId = null;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var existingVariant = db.ProductVariants.First(v => v.ProductId == productId);
            createdVariantId = existingVariant.VariantId;
        }

        var updateVariantsDto = new MerxoSell.API.DTOs.Seller.BulkUpsertVariantsDto(
            Variants: new List<MerxoSell.API.DTOs.Seller.UpsertVariantWithIdDto>
            {
                new MerxoSell.API.DTOs.Seller.UpsertVariantWithIdDto(
                    VariantId: createdVariantId,
                    Color: "Red",
                    Size: "M",
                    SKU: "RED-M-SKU",
                    PriceDelta: 0m,
                    Stock: 50,
                    IsActive: true,
                    IsDefault: true,
                    ImageUrls: new List<string> { "http://example.com/var-red.jpg" }
                )
            }
        );

        var updateVariantsResponse = await _client.PutAsJsonAsync($"/api/seller/products/{productId}/variants", updateVariantsDto);
        updateVariantsResponse.EnsureSuccessStatusCode();

        // 7. Verify: Are the variant images still there?
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var p = db.Products
                .Include(prod => prod.Variants)
                    .ThenInclude(v => v.Images)
                .First(prod => prod.ProductId == productId);
            p.Variants.Should().HaveCount(1);
            p.Variants.First().Images.Should().HaveCount(1);
            p.Variants.First().Images.First().ImageUrl.Should().Be("http://example.com/var-red.jpg");
        }

        _client.DefaultRequestHeaders.Authorization = null;
    }
}

