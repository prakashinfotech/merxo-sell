using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface ICurrencyRepository
{
    Task<IEnumerable<CurrencyRate>> GetAllActiveAsync();
    Task<IEnumerable<CurrencyRate>> GetAllAsync();
    Task<CurrencyRate?>             GetByCodeAsync(string currencyCode);
    Task<bool>                      ExistsAsync(string currencyCode);
    Task<CurrencyRate>              CreateAsync(CurrencyRate currencyRate);
    Task<CurrencyRate>              UpdateRateAsync(CurrencyRate currencyRate);
    Task<bool>                      SetActiveAsync(string currencyCode, bool isActive);
}
