using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Seller;
using MerxoSell.API.DTOs.Orders;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>Seller-scoped product management. All operations verify JWT sellerId ownership.</summary>
[Route("api/seller")]
[Authorize(Roles = "Seller")]
public class SellerProductsController : BaseApiController
{
    private readonly ISellerProductRepository _productRepo;
    private readonly ISellerRepository        _sellerRepo;
    private readonly IOrderService            _orderService;
    private readonly AppDbContext             _db;
    private readonly IMediaService            _mediaService;
    private readonly ILogger<SellerProductsController> _logger;

    public SellerProductsController(
        ISellerProductRepository productRepo,
        ISellerRepository sellerRepo,
        IOrderService orderService,
        AppDbContext db,
        IMediaService mediaService,
        ILogger<SellerProductsController> logger)
    {
        _productRepo  = productRepo;
        _sellerRepo   = sellerRepo;
        _orderService = orderService;
        _db           = db;
        _mediaService = mediaService;
        _logger       = logger;
    }

    // Extracts sellerId from JWT claim; throws if missing (should never happen for Seller role).
    private int GetSellerIdFromToken()
    {
        var claim = User.FindFirstValue("sellerId");
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var id))
            throw new UnauthorizedAccessException("Seller identity not found in token.");
        return id;
    }

    /// <summary>GET /api/seller/products — own products (all statuses).</summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetMyProducts(
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        var sellerId = GetSellerIdFromToken();
        var products = await _productRepo.GetBySellerAsync(sellerId, search, status);
        var result = products.Select(p => new SellerProductListDto(
            p.ProductId, p.Name, p.Slug, p.BasePrice, p.SalePrice,
            p.Stock, p.Status, p.ApprovalNote, p.Category.Name,
            p.Manufacturer?.Name,
            p.Images.FirstOrDefault()?.ImageUrl,
            p.ViewCount, p.CreatedAt));
        return Ok(result);
    }

    /// <summary>GET /api/seller/products/{id} — single own product detail.</summary>
    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> GetMyProduct(int id)
    {
        var sellerId = GetSellerIdFromToken();
        var product  = await _productRepo.GetByIdAndSellerAsync(id, sellerId);
        if (product is null) return NotFound(new { error = $"Product {id} not found." });

        // Explicitly ensure images are loaded (sometimes EF tracking can be finicky with Includes)
        await _db.Entry(product).Collection(p => p.Images).LoadAsync();

        var parent = product.Category.ParentCategoryId.HasValue
            ? await _db.Categories.FindAsync(product.Category.ParentCategoryId.Value)
            : null;

        _logger.LogInformation("Product {ProductId} retrieved with {ImageCount} images after explicit load.", id, product.Images.Count);

        var imagesDtoList = product.Images
            .Select(i => new CreateProductImageDto(i.ImageUrl, i.IsPrimary))
            .ToList();

        var primaryUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl 
                      ?? product.Images.FirstOrDefault()?.ImageUrl;

        var isFashion = product.Category.IsFashion || (parent?.IsFashion ?? false);

        return Ok(new SellerProductDetailDto(
            product.ProductId,
            product.Name,
            product.Description,
            product.CategoryId,
            product.Category.Name,
            product.Category.ParentCategoryId,
            parent?.Name,
            product.ManufacturerId,
            product.Manufacturer?.Name,
            product.BasePrice,
            product.SalePrice,
            product.Stock,
            product.Status,
            product.ApprovalNote,
            product.IsActive,
            primaryUrl,
            product.CreatedAt,
            imagesDtoList,
            Colors: null,
            Sizes: null,
            IsFashion: isFashion));
    }

    /// <summary>POST /api/seller/products — create product; Status defaults to Pending.</summary>
    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateSellerProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var sellerId = GetSellerIdFromToken();

        // Slug: lowercase, replace spaces with hyphens
        var slug = $"{dto.Name.ToLowerInvariant().Replace(" ", "-")}-{Guid.NewGuid().ToString()[..8]}";

        var product = new Product
        {
            SellerId       = sellerId,
            CategoryId     = dto.CategoryId,
            ManufacturerId = dto.ManufacturerId,
            Name           = dto.Name.Trim(),
            Slug           = slug,
            Description    = dto.Description,
            BasePrice      = dto.BasePrice,
            SalePrice      = dto.SalePrice,
            Stock          = dto.Stock,
            // New seller-submitted products must default to PendingApproval and stay hidden
            // from buyer-facing listings until a SuperAdmin approves them.
            Status         = "Pending",
            IsActive       = true
        };

        if (dto.Images is { Count: > 0 })
        {
            var images = new List<ProductImage>();
            foreach (var imgDto in dto.Images)
            {
                var processedUrl = await _mediaService.ProcessImageAsync(imgDto.ImageUrl);
                images.Add(new ProductImage
                {
                    ImageUrl = processedUrl,
                    IsPrimary = imgDto.IsPrimary
                });
            }
            product.Images = images;

            // Ensure exactly one primary image
            if (!product.Images.Any(i => i.IsPrimary))
                product.Images.First().IsPrimary = true;
        }

        if (dto.Variants is { Count: > 0 })
        {
            product.Variants = dto.Variants.Select(v => new ProductVariant
            {
                Color        = v.Color,
                Size         = v.Size,
                SKU          = v.SKU,
                PriceDelta   = v.PriceDelta,
                Stock        = v.Stock
            }).ToList();
        }

        var created = await _productRepo.CreateAsync(product);
        _logger.LogInformation("Seller {SellerId} created product {ProductId}", sellerId, created.ProductId);
        return CreatedAtAction(nameof(GetMyProduct), new { id = created.ProductId },
            new { productId = created.ProductId, status = created.Status });
    }

    /// <summary>
    /// PUT /api/seller/products/{id} — update own product.
    /// If product was Approved, editing resets status back to Pending.
    /// </summary>
    [HttpPut("products/{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateSellerProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var sellerId = GetSellerIdFromToken();
        var product  = await _productRepo.GetByIdAndSellerAsync(id, sellerId);
        if (product is null) return NotFound(new { error = $"Product {id} not found." });

        // Re-fetch tracking version for update
        var tracked = await _db.Products.FindAsync(id);
        if (tracked is null) return NotFound();

        var previousEffective = tracked.SalePrice ?? tracked.BasePrice;

        tracked.Name           = dto.Name.Trim();
        tracked.CategoryId     = dto.CategoryId;
        tracked.Description    = dto.Description;
        tracked.BasePrice      = dto.BasePrice;
        tracked.SalePrice      = dto.SalePrice;
        tracked.Stock          = dto.Stock;
        tracked.ManufacturerId = dto.ManufacturerId;
        // Edits send the listing back through moderation so reviewers can verify
        // the changes don't violate marketplace policies.
        tracked.Status         = "Pending";
        tracked.UpdatedAt      = DateTime.UtcNow;

        // Sync Images
        if (dto.Images != null)
        {
            // Only remove non-variant images — variant-linked images are managed by the variant upsert endpoint.
            var variantsWithImages = await _db.ProductVariants
                .Include(v => v.Images)
                .Where(v => v.ProductId == id)
                .ToListAsync();

            var variantImageIds = variantsWithImages
                .SelectMany(v => v.Images.Select(i => i.ImageId))
                .Distinct()
                .ToHashSet();

            var existing = await _db.ProductImages
                .Where(i => i.ProductId == id)
                .ToListAsync();

            var mainImages = existing.Where(i => !variantImageIds.Contains(i.ImageId)).ToList();
            _db.ProductImages.RemoveRange(mainImages);

            var newImages = new List<ProductImage>();
            foreach (var imgDto in dto.Images)
            {
                var processedUrl = await _mediaService.ProcessImageAsync(imgDto.ImageUrl);
                newImages.Add(new ProductImage
                {
                    ProductId = id,
                    ImageUrl = processedUrl,
                    IsPrimary = imgDto.IsPrimary
                });
            }
            
            if (newImages.Any() && !newImages.Any(i => i.IsPrimary))
                newImages.First().IsPrimary = true;

            await _db.ProductImages.AddRangeAsync(newImages);
        }

        await _db.SaveChangesAsync();

        // Price-drop detection — fire-and-forget (cart price is computed live).
        var newEffective = tracked.SalePrice ?? tracked.BasePrice;
        if (newEffective < previousEffective)
        {
            var drop = HttpContext.RequestServices.GetRequiredService<MerxoSell.API.Services.IPriceDropService>();
            _ = drop.NotifyIfDroppedAsync(id, previousEffective, newEffective);
        }

        return Ok(new
        {
            productId = id,
            status    = "Pending",
            message   = "Product updated and submitted for review."
        });
    }

    /// <summary>DELETE /api/seller/products/{id} — soft-delete (deactivate) own product.</summary>
    [HttpDelete("products/{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var sellerId = GetSellerIdFromToken();
        var ok = await _productRepo.SoftDeleteAsync(id, sellerId);
        if (!ok) return NotFound(new { error = $"Product {id} not found." });
        _logger.LogInformation("Seller {SellerId} deactivated product {ProductId}", sellerId, id);
        return NoContent();
    }

    /// <summary>GET /api/seller/dashboard — product counts by status + basic order summary + top products.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var sellerId = GetSellerIdFromToken();
        var products = await _productRepo.GetBySellerAsync(sellerId, null, null);
        var productList = products.ToList();

        var orderRevenue = await _db.OrderItems
            .Where(oi => oi.Product.SellerId == sellerId)
            .SumAsync(oi => (decimal?)oi.UnitPriceCAD * oi.Quantity) ?? 0m;

        var orderCount = await _db.OrderItems
            .Where(oi => oi.Product.SellerId == sellerId)
            .Select(oi => oi.OrderId)
            .Distinct()
            .CountAsync();

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var activeProducts   = productList.Count(p => p.IsActive);
        var lowStockProducts = productList.Count(p => p.IsActive && p.Stock <= 10);
        var recentProducts   = productList.Count(p => p.CreatedAt >= thirtyDaysAgo);

        var recentSales = await _db.OrderItems
            .Where(oi => oi.Product.SellerId == sellerId && oi.Order.CreatedAt >= thirtyDaysAgo)
            .Select(oi => oi.OrderId)
            .Distinct()
            .CountAsync();

        // Top Products by Views (using BrowsingHistories as requested)
        var now = DateTime.UtcNow;
        var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);
        var endOfLastMonth   = startOfThisMonth.AddTicks(-1);

        // Fetch all relevant history records in one go
        var history = await _db.BrowsingHistories
            .AsNoTracking()
            .Where(b => b.Product.SellerId == sellerId && b.ViewedAt >= startOfLastMonth)
            .ToListAsync();

        var topProducts = productList
            .Select(p => {
                var pHistory = history.Where(h => h.ProductId == p.ProductId).ToList();
                var thisMonth = pHistory.Count(h => h.ViewedAt >= startOfThisMonth);
                var lastMonth = pHistory.Count(h => h.ViewedAt >= startOfLastMonth && h.ViewedAt <= endOfLastMonth);

                double pct = 0;
                if (lastMonth > 0)
                    pct = ((double)thisMonth - lastMonth) / lastMonth * 100;
                else if (thisMonth > 0)
                    pct = 100;

                return new TopProductDto(
                    p.ProductId,
                    p.Name,
                    p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
                    thisMonth,
                    lastMonth,
                    Math.Round(pct, 2)
                );
            })
            .OrderByDescending(p => p.ThisMonthViews)
            .Take(5)
            .ToList();

        return Ok(new SellerDashboardDto(
            TotalProducts    : productList.Count,
            ActiveProducts   : activeProducts,
            LowStockProducts : lowStockProducts,
            TotalOrders      : orderCount,
            TotalRevenueCAD  : orderRevenue,
            RecentProductAdds: recentProducts,
            RecentSales      : recentSales,
            TopProducts      : topProducts
        ));
    }

    /// <summary>GET /api/seller/orders — orders that contain at least one of this seller's products.</summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        var sellerId = GetSellerIdFromToken();

        var query = _db.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Product.SellerId == sellerId)
            .Select(oi => new
            {
                oi.OrderId,
                oi.Quantity,
                oi.UnitPriceCAD,
                Order = oi.Order,
                Buyer = oi.Order.User
            });

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Order.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x =>
                x.Buyer.FullName.Contains(s) ||
                x.Buyer.Email.Contains(s) ||
                x.Order.OrderId.ToString().Contains(s));
        }

        var grouped = await query
            .GroupBy(x => new
            {
                x.OrderId,
                Status         = x.Order.Status,
                CurrencyCode   = x.Order.CurrencyCode,
                CreatedAt      = x.Order.CreatedAt,
                BuyerName      = x.Buyer.FullName,
                BuyerEmail     = x.Buyer.Email
            })
            .Select(g => new
            {
                g.Key.OrderId,
                g.Key.Status,
                g.Key.BuyerName,
                g.Key.BuyerEmail,
                g.Key.CreatedAt,
                CurrencyCode = g.Key.CurrencyCode ?? "CAD",
                ItemCount    = g.Count(),
                SellerRevenueCAD = g.Sum(x => x.Quantity * x.UnitPriceCAD)
            })
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return Ok(grouped);
    }

    /// <summary>GET /api/seller/orders/{id} — order detail filtered to this seller's items.</summary>
    [HttpGet("orders/{id:int}")]
    public async Task<IActionResult> GetMyOrder(int id)
    {
        var sellerId = GetSellerIdFromToken();

        var owns = await _db.OrderItems
            .AsNoTracking()
            .AnyAsync(oi => oi.OrderId == id && oi.Product.SellerId == sellerId);
        if (!owns) return NotFound(new { error = $"Order #{id} not found." });

        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Address)
            .Include(o => o.Items.Where(i => i.Product.SellerId == sellerId))
                .ThenInclude(i => i.Product).ThenInclude(p => p.Images.Where(img => img.IsPrimary))
            .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order is null) return NotFound(new { error = $"Order #{id} not found." });

        var sellerItems = order.Items.Where(i => i.Product.SellerId == sellerId).Select(i => new
        {
            i.OrderItemId,
            i.ProductId,
            i.VariantId,
            ProductName     = i.Product?.Name ?? "Unknown product",
            PrimaryImageUrl = i.Product?.Images.FirstOrDefault()?.ImageUrl,
            VariantInfo     = (i.Variant != null && (i.Variant.Color != null || i.Variant.Size != null))
                                ? string.Join(", ", new[] {
                                    i.Variant.Color != null ? $"Color: {i.Variant.Color}" : null,
                                    i.Variant.Size  != null ? $"Size: {i.Variant.Size}"   : null
                                  }.Where(s => s != null))
                                : null,
            i.Quantity,
            i.UnitPriceCAD
        });

        var sellerRevenue = sellerItems.Sum(i => i.Quantity * i.UnitPriceCAD);

        return Ok(new
        {
            order.OrderId,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt,
            CurrencyCode    = order.CurrencyCode ?? "CAD",
            BuyerName       = order.User.FullName,
            BuyerEmail      = order.User.Email,
            BuyerPhone      = order.User.Phone,
            ShippingAddress = order.Address == null ? null : new
            {
                order.Address.FullName,
                order.Address.Phone,
                order.Address.AddressLine1,
                order.Address.AddressLine2,
                order.Address.City,
                order.Address.State,
                order.Address.PostalCode,
                order.Address.Country
            },
            Items            = sellerItems,
            SellerRevenueCAD = sellerRevenue
        });
    }

    /// <summary>
    /// PATCH /api/seller/orders/{id}/status — Seller updates the workflow status
    /// for an order they have items in. Recorded in OrderStatusHistory and the
    /// buyer is notified by email.
    /// </summary>
    [HttpPatch("orders/{id:int}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var sellerId = GetSellerIdFromToken();
        var owns = await _db.OrderItems
            .AsNoTracking()
            .AnyAsync(oi => oi.OrderId == id && oi.Product.SellerId == sellerId);
        if (!owns) return NotFound(new { error = $"Order #{id} not found." });

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _orderService.UpdateOrderStatusAsync(id, dto.Status, dto.Note, userId, "Seller");
            return Ok(new { message = $"Order #{id} updated to {dto.Status}." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>POST /api/seller/orders/{id}/cancel — Seller cancels with required reason.</summary>
    [HttpPost("orders/{id:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var sellerId = GetSellerIdFromToken();
        var owns = await _db.OrderItems
            .AsNoTracking()
            .AnyAsync(oi => oi.OrderId == id && oi.Product.SellerId == sellerId);
        if (!owns) return NotFound(new { error = $"Order #{id} not found." });

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _orderService.CancelOrderAsync(id, dto.CancellationReason, userId, "Seller");
            return Ok(new { message = $"Order #{id} cancelled." });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>GET /api/seller/orders/{id}/history — full status timeline (seller scope).</summary>
    [HttpGet("orders/{id:int}/history")]
    public async Task<IActionResult> GetOrderHistory(int id)
    {
        var sellerId = GetSellerIdFromToken();
        var owns = await _db.OrderItems
            .AsNoTracking()
            .AnyAsync(oi => oi.OrderId == id && oi.Product.SellerId == sellerId);
        if (!owns) return NotFound(new { error = $"Order #{id} not found." });

        var rows = await _orderService.GetStatusHistoryAsync(id);
        return Ok(rows);
    }

    // ── Variant management ────────────────────────────────────────────────
    // Sellers maintain product variants here. Bulk-upsert is the canonical
    // operation — the form-builder UI submits the full variant matrix each
    // time and the server reconciles inserts/updates/deletes.

    /// <summary>GET /api/seller/products/{id}/variants — variant matrix for the seller dashboard.</summary>
    [HttpGet("products/{id:int}/variants")]
    public async Task<IActionResult> GetVariants(int id)
    {
        var sellerId = GetSellerIdFromToken();
        var owns = await _db.Products.AsNoTracking()
            .AnyAsync(p => p.ProductId == id && p.SellerId == sellerId);
        if (!owns) return NotFound(new { error = $"Product {id} not found." });

        var variants = await _db.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == id)
            .OrderByDescending(v => v.IsDefault)
            .ThenBy(v => v.Color).ThenBy(v => v.Size)
            .Select(v => new
            {
                v.VariantId, v.Color, v.Size, v.SKU,
                v.PriceDelta, v.Stock, v.IsActive, v.IsDefault,
                Images = v.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new MerxoSell.API.DTOs.Seller.SellerVariantImageDto(
                        i.ImageId, i.ImageUrl, i.IsPrimary, i.SortOrder))
                    .ToList(),
            })
            .ToListAsync();

        var result = variants.Select(v => new MerxoSell.API.DTOs.Seller.SellerVariantDto(
            v.VariantId, v.Color, v.Size, v.SKU,
            v.PriceDelta, v.Stock, v.IsActive, v.IsDefault,
            v.Images));

        return Ok(result);
    }

    /// <summary>
    /// PUT /api/seller/products/{id}/variants — replace the full variant list.
    /// Variants with an existing VariantId are updated, new rows are inserted,
    /// and existing variants not in the payload are removed. Image URLs are
    /// reconciled per variant (delete-then-insert keeps the code simple).
    /// </summary>
    [HttpPut("products/{id:int}/variants")]
    public async Task<IActionResult> ReplaceVariants(int id,
        [FromBody] MerxoSell.API.DTOs.Seller.BulkUpsertVariantsDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var sellerId = GetSellerIdFromToken();
        var product = await _db.Products
            .Include(p => p.Variants)
            .ThenInclude(v => v.Images)
            .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == sellerId);
        if (product is null) return NotFound(new { error = $"Product {id} not found." });

        // Validate: enforce at most one IsDefault and unique (Color, Size) combinations.
        var defaults = dto.Variants.Count(v => v.IsDefault);
        if (defaults > 1)
            return BadRequest(new { error = "Only one variant can be marked as default." });

        var dupKey = dto.Variants
            .GroupBy(v => $"{v.Color?.Trim()?.ToLowerInvariant()}|{v.Size?.Trim()?.ToLowerInvariant()}")
            .FirstOrDefault(g => g.Count() > 1);
        if (dupKey is not null)
            return BadRequest(new { error = "Two variants share the same colour + size combination." });

        var tx = _db.Database.IsRelational() ? await _db.Database.BeginTransactionAsync() : null;
        try
        {
            // 1) Delete variants not in the payload (join table rows are cascade-deleted).
            var keepIds = dto.Variants.Where(v => v.VariantId.HasValue).Select(v => v.VariantId!.Value).ToHashSet();
            var toRemove = product.Variants.Where(v => !keepIds.Contains(v.VariantId)).ToList();
            if (toRemove.Count > 0)
            {
                _db.ProductVariants.RemoveRange(toRemove);
            }

            // 2) Update existing + 3) Insert new
            foreach (var v in dto.Variants)
            {
                if (v.VariantId.HasValue)
                {
                    var existing = product.Variants.FirstOrDefault(x => x.VariantId == v.VariantId.Value);
                    if (existing is null) continue;
                    existing.Color      = v.Color;
                    existing.Size       = v.Size;
                    existing.SKU        = v.SKU;
                    existing.PriceDelta = v.PriceDelta;
                    existing.Stock      = v.Stock;
                    existing.IsActive   = v.IsActive;
                    existing.IsDefault  = v.IsDefault;
                    SyncVariantImages(existing, v.ImageUrls);
                }
                else
                {
                    var fresh = new MerxoSell.API.Models.ProductVariant
                    {
                        ProductId  = id,
                        Color      = v.Color,
                        Size       = v.Size,
                        SKU        = v.SKU,
                        PriceDelta = v.PriceDelta,
                        Stock      = v.Stock,
                        IsActive   = v.IsActive,
                        IsDefault  = v.IsDefault,
                    };
                    product.Variants.Add(fresh);
                    SyncVariantImages(fresh, v.ImageUrls);
                }
            }

            await _db.SaveChangesAsync();
            if (tx is not null) await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            if (tx is not null) await tx.RollbackAsync();
            _logger.LogError(ex, "Failed to replace variants for product {ProductId}", id);
            return StatusCode(500, new { error = "Could not save variants. Please try again." });
        }
        finally
        {
            if (tx is not null) await tx.DisposeAsync();
        }

        return Ok(new { message = "Variants saved.", productId = id });
    }

    private void SyncVariantImages(MerxoSell.API.Models.ProductVariant variant, IList<string>? urls)
    {
        if (urls is null)
        {
            variant.Images.Clear();
            return;
        }

        var desiredUrls = urls.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()).ToHashSet();

        // Remove images no longer desired for this variant
        var toRemove = variant.Images.Where(i => !desiredUrls.Contains(i.ImageUrl)).ToList();
        foreach (var img in toRemove)
        {
            variant.Images.Remove(img);
        }

        // Add desired images not currently in variant.Images
        var order = 0;
        foreach (var url in desiredUrls)
        {
            if (!variant.Images.Any(i => i.ImageUrl == url))
            {
                var img = _db.ProductImages.Local.FirstOrDefault(i => i.ProductId == variant.ProductId && i.ImageUrl == url)
                       ?? _db.ProductImages.FirstOrDefault(i => i.ProductId == variant.ProductId && i.ImageUrl == url);

                if (img == null)
                {
                    img = new MerxoSell.API.Models.ProductImage
                    {
                        ProductId = variant.ProductId,
                        ImageUrl = url,
                        IsPrimary = order == 0,
                        SortOrder = order++,
                    };
                    _db.ProductImages.Add(img);
                }
                variant.Images.Add(img);
            }
        }
    }

    /// <summary>GET /api/seller/products/{id}/reviews — reviews for seller's own product.</summary>
    [HttpGet("products/{id:int}/reviews")]
    public async Task<IActionResult> GetProductReviews(int id)
    {
        var sellerId = GetSellerIdFromToken();
        var exists   = await _db.Products.AnyAsync(p => p.ProductId == id && p.SellerId == sellerId);
        if (!exists) return NotFound(new { error = $"Product {id} not found." });

        var reviews = await _db.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ProductId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.ReviewId, r.Rating, r.Comment,
                UserName = r.User.FullName,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(reviews);
    }
}
