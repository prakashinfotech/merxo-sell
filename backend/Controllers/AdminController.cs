using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Admin;
using MerxoSell.API.DTOs.Products;
using MerxoSell.API.Services;
using MerxoSell.API.Services.Interfaces;
using MerxoSell.API.Models;

namespace MerxoSell.API.Controllers;

/// <summary>SuperAdmin-only dashboard stats and product management.</summary>
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : BaseApiController
{
    private readonly AppDbContext      _db;
    private readonly IProductService   _productService;
    private readonly IPriceDropService _priceDrop;

    public AdminController(AppDbContext db, IProductService productService, IPriceDropService priceDrop)
    {
        _db             = db;
        _productService = productService;
        _priceDrop      = priceDrop;
    }

    /// <summary>GET /api/admin/stats — dashboard statistics.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = new AdminStatsDto
        {
            TotalProducts      = await _db.Products.CountAsync(p => !p.IsDeleted),
            ActiveProducts     = await _db.Products.CountAsync(p => p.IsActive && !p.IsDeleted),
            LowStockProducts   = await _db.Products.CountAsync(p => p.Stock <= 10 && p.IsActive && !p.IsDeleted),
            TotalUsers         = await _db.Users.CountAsync(u => u.IsActive && !u.IsDeleted),
            TotalManufacturers = await _db.Manufacturers.CountAsync(m => m.IsActive),
            TotalOrders        = await _db.Orders.CountAsync(),
            TotalRevenueCAD    = await _db.Orders.SumAsync(o => (decimal?)o.TotalAmountCAD) ?? 0m
        };
        return Ok(stats);
    }

    /// <summary>GET /api/admin/products — all products (including inactive) for admin management.</summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] string? search, [FromQuery] int? manufacturerId)
    {
        var query = _db.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        if (manufacturerId.HasValue)
            query = query.Where(p => p.ManufacturerId == manufacturerId.Value);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new AdminProductListDto
            {
                ProductId        = p.ProductId,
                Name             = p.Name,
                Slug             = p.Slug,
                BasePrice        = p.BasePrice,
                SalePrice        = p.SalePrice,
                Stock            = p.Stock,
                IsActive         = p.IsActive,
                CategoryName     = p.Category.Name,
                ManufacturerId   = p.ManufacturerId,
                ManufacturerName = p.Manufacturer != null ? p.Manufacturer.Name : null,
                PrimaryImageUrl  = p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
                CreatedAt        = p.CreatedAt
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>GET /api/admin/products/{id} — single product full detail for admin.</summary>
    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetDetailAsync(id);
        return product is null
            ? NotFound(new { error = $"Product {id} not found." })
            : Ok(product);
    }

    /// <summary>PUT /api/admin/products/{id} — update product.</summary>
    [HttpPut("products/{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
            return NotFound(new { error = $"Product {id} not found." });

        // Snapshot the effective price BEFORE applying changes so we can detect a drop.
        var previousEffective = product.SalePrice ?? product.BasePrice;

        product.Name           = dto.Name;
        product.Description    = dto.Description;
        product.BasePrice      = dto.BasePrice;
        product.SalePrice      = dto.SalePrice;
        product.Stock          = dto.Stock;
        product.IsActive       = dto.IsActive;
        product.ManufacturerId = dto.ManufacturerId;
        product.UpdatedAt      = DateTime.UtcNow;

        // Handle Primary Image Update
        if (!string.IsNullOrWhiteSpace(dto.PrimaryImageUrl))
        {
            var primaryImg = await _db.ProductImages
                .FirstOrDefaultAsync(i => i.ProductId == id && i.IsPrimary);
            
            if (primaryImg != null)
            {
                primaryImg.ImageUrl = dto.PrimaryImageUrl;
            }
            else
            {
                _db.ProductImages.Add(new ProductImage 
                { 
                    ProductId = id, 
                    ImageUrl = dto.PrimaryImageUrl, 
                    IsPrimary = true 
                });
            }
        }

        await _db.SaveChangesAsync();

        // Price-drop detection — runs out-of-band so a slow email send doesn't
        // delay the response. Cart items always show the live product price so
        // there's nothing extra to sync server-side.
        var newEffective = product.SalePrice ?? product.BasePrice;
        if (newEffective < previousEffective)
            _ = _priceDrop.NotifyIfDroppedAsync(id, previousEffective, newEffective);

        return Ok(new { message = "Product updated.", productId = id });
    }

    [HttpDelete("products/{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
            return NotFound(new { error = $"Product {id} not found." });

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
