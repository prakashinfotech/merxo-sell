using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly AppDbContext _db;

    public CurrencyRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<CurrencyRate>> GetAllActiveAsync()
        => await _db.CurrencyRates
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CurrencyCode)
                    .ToListAsync();

    public async Task<IEnumerable<CurrencyRate>> GetAllAsync()
        => await _db.CurrencyRates
                    .AsNoTracking()
                    .OrderBy(c => c.CurrencyCode)
                    .ToListAsync();

    public async Task<CurrencyRate?> GetByCodeAsync(string currencyCode)
        => await _db.CurrencyRates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CurrencyCode == currencyCode);

    public async Task<bool> ExistsAsync(string currencyCode)
        => await _db.CurrencyRates
                    .AnyAsync(c => c.CurrencyCode == currencyCode);

    public async Task<CurrencyRate> CreateAsync(CurrencyRate currencyRate)
    {
        currencyRate.LastUpdated = DateTime.UtcNow;
        _db.CurrencyRates.Add(currencyRate);
        await _db.SaveChangesAsync();
        return currencyRate;
    }

    public async Task<CurrencyRate> UpdateRateAsync(CurrencyRate currencyRate)
    {
        var existing = await _db.CurrencyRates
            .FirstOrDefaultAsync(c => c.CurrencyCode == currencyRate.CurrencyCode)
            ?? throw new KeyNotFoundException($"Currency '{currencyRate.CurrencyCode}' not found.");

        existing.CurrencyName = currencyRate.CurrencyName;
        existing.RateToCad    = currencyRate.RateToCad;
        existing.Symbol       = currencyRate.Symbol;
        existing.IsActive     = currencyRate.IsActive;
        existing.LastUpdated  = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> SetActiveAsync(string currencyCode, bool isActive)
    {
        var rate = await _db.CurrencyRates.FirstOrDefaultAsync(c => c.CurrencyCode == currencyCode);
        if (rate is null) return false;

        rate.IsActive    = isActive;
        rate.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
