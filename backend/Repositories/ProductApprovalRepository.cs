using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class ProductApprovalRepository : IProductApprovalRepository
{
    private readonly AppDbContext _db;
    public ProductApprovalRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Product>> GetPendingQueueAsync() =>
        await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Seller).ThenInclude(s => s.User)
            .Include(p => p.Images.Where(i => i.IsPrimary))
            .Where(p => p.Status == "Pending" && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    /// <summary>
    /// Approve: set Status=Approved + insert audit log in one transaction.
    /// </summary>
    public async Task ApproveAsync(int productId, int reviewerUserId, string? note)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        var product = await _db.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        var oldStatus = product.Status;
        product.Status       = "Approved";
        product.ApprovalNote = note;
        product.UpdatedAt    = DateTime.UtcNow;

        _db.ProductApprovalLogs.Add(new ProductApprovalLog
        {
            ProductId  = productId,
            ReviewedBy = reviewerUserId,
            OldStatus  = oldStatus,
            NewStatus  = "Approved",
            Note       = note,
            CreatedAt  = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    /// <summary>
    /// Reject: set Status=Rejected + insert audit log in one transaction.
    /// Note is required for rejections.
    /// </summary>
    public async Task RejectAsync(int productId, int reviewerUserId, string note)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        var product = await _db.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        var oldStatus = product.Status;
        product.Status       = "Rejected";
        product.ApprovalNote = note;
        product.UpdatedAt    = DateTime.UtcNow;

        _db.ProductApprovalLogs.Add(new ProductApprovalLog
        {
            ProductId  = productId,
            ReviewedBy = reviewerUserId,
            OldStatus  = oldStatus,
            NewStatus  = "Rejected",
            Note       = note,
            CreatedAt  = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task<IEnumerable<ProductApprovalLog>> GetHistoryAsync(int? productId, int page, int pageSize)
    {
        var q = _db.ProductApprovalLogs
            .AsNoTracking()
            .Include(l => l.Product)
            .Include(l => l.Reviewer)
            .AsQueryable();

        if (productId.HasValue)
            q = q.Where(l => l.ProductId == productId.Value);

        return await q.OrderByDescending(l => l.CreatedAt)
                      .Skip((page - 1) * pageSize)
                      .Take(pageSize)
                      .ToListAsync();
    }
}
