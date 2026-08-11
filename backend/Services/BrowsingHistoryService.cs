using MerxoSell.API.DTOs.BrowsingHistory;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class BrowsingHistoryService : IBrowsingHistoryService
{
    private readonly IBrowsingHistoryRepository _historyRepo;
    private readonly IProductRepository _productRepo;

    public BrowsingHistoryService(IBrowsingHistoryRepository historyRepo, IProductRepository productRepo)
    {
        _historyRepo = historyRepo;
        _productRepo = productRepo;
    }

    public async Task RecordViewAsync(int? userId, int productId)
    {
        var product = await _productRepo.GetDetailAsync(productId);
        if (product is null) throw new KeyNotFoundException("Product not found.");

        await _historyRepo.RecordViewAsync(userId, productId);
    }

    public async Task<IReadOnlyList<BrowsingHistoryDto>> GetForUserAsync(int userId, int page, int pageSize)
    {
        var take = Math.Clamp(pageSize, 1, 48);
        var skip = (Math.Clamp(page, 1, int.MaxValue) - 1) * take;
        var rows = await _historyRepo.GetForUserAsync(userId, skip, take);

        return rows
            .Select(h => new BrowsingHistoryDto(
                h.BrowsingHistoryId,
                h.ProductId,
                h.Product.Name,
                h.Product.Images.FirstOrDefault()?.ImageUrl,
                h.Product.BasePrice,
                h.Product.SalePrice,
                h.ViewedAt))
            .ToList();
    }

    public Task<bool> DeleteAsync(int userId, int browsingHistoryId)
        => _historyRepo.DeleteAsync(userId, browsingHistoryId);

    public Task ClearAsync(int userId) => _historyRepo.ClearForUserAsync(userId);
}
