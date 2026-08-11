using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _db;
    public ReviewRepository(AppDbContext db) => _db = db;

    public async Task<Review> CreateReviewAsync(Review review)
    {
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return review;
    }

    public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
    {
        // Buyer-facing list: only Approved reviews.
        return await _db.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.Status == "Approved")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByUserIdAsync(int userId)
    {
        return await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Product)
                .ThenInclude(p => p.Images)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetAllForPortalAsync(string? search, int? rating)
    {
        var q = PortalQuery();
        q = ApplyFilters(q, search, rating);
        return await q.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetBySellerAsync(int sellerId, string? search, int? rating)
    {
        var q = PortalQuery().Where(r => r.Product.SellerId == sellerId);
        q = ApplyFilters(q, search, rating);
        return await q.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
    {
        return await _db.OrderItems
            .AnyAsync(oi => oi.Order.UserId == userId && oi.ProductId == productId);
    }

    public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
    {
        return await _db.Reviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
    }

    public async Task<IEnumerable<Review>> GetFlaggedAsync()
    {
        return await PortalQuery()
            .Where(r => r.Status == "Flagged")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetByIdForModerationAsync(int reviewId)
    {
        return await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
    }

    public async Task UpdateModerationStatusAsync(Review review)
    {
        _db.Reviews.Update(review);
        await _db.SaveChangesAsync();
    }

    private IQueryable<Review> PortalQuery() =>
        _db.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
                .ThenInclude(p => p.Seller);

    private static IQueryable<Review> ApplyFilters(IQueryable<Review> q, string? search, int? rating)
    {
        if (rating is >= 1 and <= 5)
            q = q.Where(r => r.Rating == rating);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(r =>
                r.Product.Name.Contains(s) ||
                r.User.FullName.Contains(s) ||
                r.User.Email.Contains(s) ||
                (r.Comment != null && r.Comment.Contains(s)));
        }

        return q;
    }
}
