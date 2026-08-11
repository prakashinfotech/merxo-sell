using MerxoSell.API.DTOs.Currency;

namespace MerxoSell.API.Services.Interfaces;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyRateDto>> GetAllActiveAsync();
    Task<CurrencyRateDto>              UpdateRateAsync(string currencyCode, decimal newRate);
}
