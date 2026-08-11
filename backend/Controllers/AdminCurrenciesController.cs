using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Currency;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>SuperAdmin-only currency rate management. CAD is the immutable base.</summary>
[Route("api/admin/rates")]
[Authorize(Roles = "SuperAdmin")]
[ApiController]
public class AdminCurrenciesController : ControllerBase
{
    private const string BaseCurrency = "CAD";

    private readonly ICurrencyRepository                  _repo;
    private readonly ILogger<AdminCurrenciesController>   _logger;

    public AdminCurrenciesController(
        ICurrencyRepository repo,
        ILogger<AdminCurrenciesController> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    /// <summary>GET /api/admin/currencies — list every currency including inactive ones.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rates = await _repo.GetAllAsync();
        var result = rates.Select(c => new AdminCurrencyDto(
            c.CurrencyCode, c.CurrencyName, c.RateToCad, c.Symbol, c.IsActive, c.LastUpdated));
        return Ok(result);
    }

    /// <summary>GET /api/admin/currencies/{code} — single currency lookup.</summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var c = await _repo.GetByCodeAsync(code.ToUpperInvariant());
        if (c is null) return NotFound(new { error = $"Currency '{code}' not found." });

        return Ok(new AdminCurrencyDto(
            c.CurrencyCode, c.CurrencyName, c.RateToCad, c.Symbol, c.IsActive, c.LastUpdated));
    }

    /// <summary>POST /api/admin/currencies — add a new currency.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.CurrencyCode.ToUpperInvariant().Trim();
        if (await _repo.ExistsAsync(code))
            return Conflict(new { error = $"Currency '{code}' already exists." });

        if (code == BaseCurrency && dto.RateToCad != 1m)
            return BadRequest(new { error = "CAD must have a rate of 1.000000." });

        var entity = new CurrencyRate
        {
            CurrencyCode = code,
            CurrencyName = dto.CurrencyName.Trim(),
            RateToCad    = dto.RateToCad,
            Symbol       = dto.Symbol.Trim(),
            IsActive     = dto.IsActive
        };

        var created = await _repo.CreateAsync(entity);
        _logger.LogInformation("Currency {Code} created at rate {Rate}", code, dto.RateToCad);

        return CreatedAtAction(nameof(GetByCode), new { code = created.CurrencyCode },
            new AdminCurrencyDto(created.CurrencyCode, created.CurrencyName,
                created.RateToCad, created.Symbol, created.IsActive, created.LastUpdated));
    }

    /// <summary>PUT /api/admin/currencies/{code} — update full record (rate, name, symbol, status).</summary>
    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateCurrencyDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var upper = code.ToUpperInvariant();
        if (upper == BaseCurrency)
        {
            if (dto.RateToCad != 1m)
                return BadRequest(new { error = "CAD rate must remain 1.000000." });
            if (!dto.IsActive)
                return BadRequest(new { error = "CAD is the base currency and cannot be deactivated." });
        }

        var existing = await _repo.GetByCodeAsync(upper);
        if (existing is null) return NotFound(new { error = $"Currency '{code}' not found." });

        existing.CurrencyName = dto.CurrencyName.Trim();
        existing.RateToCad    = dto.RateToCad;
        existing.Symbol       = dto.Symbol.Trim();
        existing.IsActive     = dto.IsActive;

        var updated = await _repo.UpdateRateAsync(existing);

        _logger.LogInformation("Currency {Code} updated to rate {Rate}", upper, dto.RateToCad);
        return Ok(new AdminCurrencyDto(updated.CurrencyCode, updated.CurrencyName,
            updated.RateToCad, updated.Symbol, updated.IsActive, updated.LastUpdated));
    }

    /// <summary>DELETE /api/admin/currencies/{code} — soft-delete (deactivate).</summary>
    [HttpDelete("{code}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(string code)
    {
        var upper = code.ToUpperInvariant();
        if (upper == BaseCurrency)
            return BadRequest(new { error = "CAD is the base currency and cannot be removed." });

        var ok = await _repo.SetActiveAsync(upper, false);
        if (!ok) return NotFound(new { error = $"Currency '{code}' not found." });

        _logger.LogInformation("Currency {Code} deactivated by admin", upper);
        return NoContent();
    }
}
