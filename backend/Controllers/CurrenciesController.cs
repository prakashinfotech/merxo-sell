using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Currency;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>Public read-only currency data used by the Angular display layer.</summary>
[Route("api/currencies")]
public class CurrenciesController : BaseApiController
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    /// <summary>GET /api/currencies — returns all active currencies with their CAD rates.</summary>
    [HttpGet]
    public async Task<ActionResult<List<CurrencyRateDto>>> GetAllAsync()
    {
        var rates = await _currencyService.GetAllActiveAsync();
        return Ok(rates);
    }
}
