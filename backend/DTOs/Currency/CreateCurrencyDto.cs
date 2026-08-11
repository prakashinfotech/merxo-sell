using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Currency;

public record CreateCurrencyDto(
    [Required, MaxLength(10), MinLength(2)] string CurrencyCode,
    [Required, MaxLength(100)]               string CurrencyName,
    [Range(0.000001, 1_000_000)]             decimal RateToCad,
    [Required, MaxLength(10)]                string Symbol,
    bool                                      IsActive = true
);

public record UpdateCurrencyDto(
    [Required, MaxLength(100)]               string CurrencyName,
    [Range(0.000001, 1_000_000)]             decimal RateToCad,
    [Required, MaxLength(10)]                string Symbol,
    [Required]                               bool   IsActive
);

public record AdminCurrencyDto(
    string   CurrencyCode,
    string   CurrencyName,
    decimal  RateToCad,
    string   Symbol,
    bool     IsActive,
    DateTime LastUpdated
);
