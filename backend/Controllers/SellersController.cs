using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Seller;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Email;

namespace MerxoSell.API.Controllers;

/// <summary>SuperAdmin management of seller accounts.</summary>
[Route("api/admin/sellers")]
[Authorize(Roles = "SuperAdmin")]
public class SellersController : BaseApiController
{
    private readonly ISellerRepository _sellerRepo;
    private readonly AppDbContext      _db;
    private readonly IEmailService     _email;
    private readonly ILogger<SellersController> _logger;

    public SellersController(ISellerRepository sellerRepo, AppDbContext db,
        IEmailService email,
        ILogger<SellersController> logger)
    {
        _sellerRepo = sellerRepo;
        _db         = db;
        _email      = email;
        _logger     = logger;
    }

    /// <summary>GET /api/admin/sellers — list all sellers with optional search and status filter.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? isActive)
    {
        var sellers = await _sellerRepo.GetAllAsync(search, isActive);
        var result = new List<SellerDto>();
        
        foreach (var s in sellers)
        {
            var productCount = await _db.Products.CountAsync(p => p.SellerId == s.SellerId);
            result.Add(new SellerDto(
                s.SellerId, s.UserId, s.StoreName, s.StoreDescription,
                s.ContactEmail, s.Phone, s.IsVerified, s.IsActive,
                s.User.Email, s.User.FullName, productCount, s.CreatedAt));
        }
        
        return Ok(result);
    }

    /// <summary>GET /api/admin/sellers/{id} — single seller detail.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _sellerRepo.GetByIdAsync(id);
        if (s is null) return NotFound(new { error = $"Seller {id} not found." });
        var productCount = await _db.Products.CountAsync(p => p.SellerId == id);
        return Ok(new SellerDto(s.SellerId, s.UserId, s.StoreName, s.StoreDescription,
            s.ContactEmail, s.Phone, s.IsVerified, s.IsActive,
            s.User.Email, s.User.FullName, productCount, s.CreatedAt));
    }

    /// <summary>GET /api/admin/sellers/{id}/products — all products for a seller.</summary>
    [HttpGet("{id:int}/products")]
    public async Task<IActionResult> GetSellerProducts(int id, [FromQuery] string? status)
    {
        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Where(p => p.SellerId == id && (status == null || p.Status == status))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new SellerProductListDto(
                p.ProductId, p.Name, p.Slug, p.BasePrice, p.SalePrice,
                p.Stock, p.Status, p.ApprovalNote, p.Category.Name,
                p.Manufacturer != null ? p.Manufacturer.Name : null,
                p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
                p.ViewCount, p.CreatedAt))
            .ToListAsync();
        return Ok(products);
    }

    /// <summary>POST /api/admin/sellers — create a seller record for an existing user.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateSellerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userExists = await _db.Users.AnyAsync(u => u.UserId == dto.UserId && u.IsActive);
        if (!userExists) return BadRequest(new { error = "User not found or inactive." });

        if (await _sellerRepo.ExistsByUserIdAsync(dto.UserId))
            return Conflict(new { error = "A seller record already exists for this user." });

        var seller = new Seller
        {
            UserId           = dto.UserId,
            StoreName        = dto.StoreName.Trim(),
            StoreDescription = dto.StoreDescription,
            ContactEmail     = dto.ContactEmail,
            Phone            = dto.Phone,
            IsVerified       = false,
            IsActive         = true
        };

        var created = await _sellerRepo.CreateAsync(seller);
        _logger.LogInformation("Seller {SellerId} created for user {UserId}", created.SellerId, dto.UserId);
        return CreatedAtAction(nameof(GetById), new { id = created.SellerId },
            new { sellerId = created.SellerId, message = "Seller created." });
    }

    /// <summary>PUT /api/admin/sellers/{id} — update seller info.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSellerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var seller = await _db.Sellers.FindAsync(id);
        if (seller is null) return NotFound(new { error = $"Seller {id} not found." });

        seller.StoreName        = dto.StoreName.Trim();
        seller.StoreDescription = dto.StoreDescription;
        seller.ContactEmail     = dto.ContactEmail;
        seller.Phone            = dto.Phone;
        seller.UpdatedAt        = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Seller updated." });
    }

    /// <summary>PATCH /api/admin/sellers/{id}/status — activate or deactivate seller.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetSellerStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var ok = await _sellerRepo.SetStatusAsync(id, dto.IsActive);
        if (!ok) return NotFound(new { error = $"Seller {id} not found." });
        _logger.LogInformation("Seller {SellerId} status set to {IsActive}", id, dto.IsActive);

        var s = await _db.Sellers.Include(x => x.User).FirstOrDefaultAsync(x => x.SellerId == id);
        if (s != null && s.User != null)
            _ = _email.SendAccountStatusNotificationAsync(s.User.Email, s.User.FullName, dto.IsActive, s.User.IsDeleted);

        return Ok(new { message = $"Seller {(dto.IsActive ? "activated" : "deactivated")}." });
    }

    /// <summary>PATCH /api/admin/sellers/{id}/verify — toggle the verified badge.</summary>
    [HttpPatch("{id:int}/verify")]
    public async Task<IActionResult> SetVerified(int id, [FromBody] SetSellerVerifiedDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var seller = await _db.Sellers.FindAsync(id);
        if (seller is null) return NotFound(new { error = $"Seller {id} not found." });

        seller.IsVerified = dto.IsVerified;
        seller.UpdatedAt  = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Seller {SellerId} verified set to {IsVerified}", id, dto.IsVerified);
        return Ok(new { message = $"Seller verification {(dto.IsVerified ? "granted" : "revoked")}." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _sellerRepo.SoftDeleteAsync(id);
        if (!ok) return NotFound(new { error = $"Seller {id} not found." });
        _logger.LogInformation("Seller {SellerId} soft-deleted by admin", id);

        var s = await _db.Sellers.Include(x => x.User).FirstOrDefaultAsync(x => x.SellerId == id);
        if (s != null && s.User != null)
            _ = _email.SendAccountStatusNotificationAsync(s.User.Email, s.User.FullName, s.IsActive, true);

        return NoContent();
    }
}
