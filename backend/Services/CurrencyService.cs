using MerxoSell.API.DTOs.Currency;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _repo;

    public CurrencyService(ICurrencyRepository repo) => _repo = repo;

    public async Task<IEnumerable<CurrencyRateDto>> GetAllActiveAsync()
    {
        var rates = await _repo.GetAllActiveAsync();
        return rates.Select(ToDto);
    }

    public async Task<CurrencyRateDto> UpdateRateAsync(string currencyCode, decimal newRate)
    {
        var rate = await _repo.GetByCodeAsync(currencyCode)
            ?? throw new KeyNotFoundException($"Currency '{currencyCode}' not found.");

        rate.RateToCad = newRate;
        var updated = await _repo.UpdateRateAsync(rate);
        return ToDto(updated);
    }

    private static CurrencyRateDto ToDto(Models.CurrencyRate c) => new()
    {
        CurrencyCode = c.CurrencyCode,
        CurrencyName = c.CurrencyName,
        RateToCad    = c.RateToCad,
        Symbol       = c.Symbol,
        LastUpdated  = c.LastUpdated
    };
}
