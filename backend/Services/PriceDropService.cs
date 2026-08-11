using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MerxoSell.API.Data;
using MerxoSell.API.Services.Email;

namespace MerxoSell.API.Services;

/// <summary>
/// Fired whenever a product's effective price decreases. Notifies every buyer
/// who currently has the product in their cart. The work runs in a brand-new
/// DI scope (fire-and-forget from the controller) so it never reuses the
/// request's DbContext after the response has been written.
/// </summary>
public interface IPriceDropService
{
    Task NotifyIfDroppedAsync(
        int     productId,
        decimal previousEffectivePrice,
        decimal newEffectivePrice);
}

public sealed class PriceDropService : IPriceDropService
{
    private readonly IServiceScopeFactory       _scopeFactory;
    private readonly ILogger<PriceDropService>  _logger;

    public PriceDropService(IServiceScopeFactory scopeFactory, ILogger<PriceDropService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task NotifyIfDroppedAsync(
        int productId, decimal previousEffectivePrice, decimal newEffectivePrice)
    {
        if (newEffectivePrice >= previousEffectivePrice) return;

        // Always own our DbContext + EmailService instances. The caller fires
        // this method without awaiting, so the request scope can be torn down
        // at any moment — sharing its DbContext would race the response pipe.
        using var scope = _scopeFactory.CreateScope();
        var db    = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            var rows = await db.CartItems
                .AsNoTracking()
                .Where(ci => ci.ProductId == productId)
                .Select(ci => new
                {
                    ci.Cart.User.UserId,
                    ci.Cart.User.Email,
                    ci.Cart.User.FullName,
                    ci.Cart.User.PreferredCurrency,
                    ProductName = ci.Product.Name,
                    Image = ci.Product.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                })
                .Distinct()
                .ToListAsync();

            if (rows.Count == 0) return;

            var currencyByCode = await db.CurrencyRates
                .AsNoTracking()
                .ToDictionaryAsync(c => c.CurrencyCode, c => c.RateToCad);

            foreach (var row in rows)
            {
                try
                {
                    var rate = row.PreferredCurrency == "CAD" 
                        ? 1m 
                        : (currencyByCode.TryGetValue(row.PreferredCurrency, out var r) ? r : 1m);
                    
                    var oldDisplay = previousEffectivePrice * rate;
                    var newDisplay = newEffectivePrice      * rate;

                    await email.SendPriceDropAsync(
                        toEmail:          row.Email,
                        fullName:         row.FullName,
                        productName:      row.ProductName,
                        productImageUrl:  row.Image,
                        oldPriceCAD:      previousEffectivePrice,
                        newPriceCAD:      newEffectivePrice,
                        currencyCode:     row.PreferredCurrency,
                        oldPriceDisplay:  oldDisplay,
                        newPriceDisplay:  newDisplay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Price-drop email failed for user {UserId}, product {ProductId}",
                        row.UserId, productId);
                }
            }

            _logger.LogInformation(
                "Price drop notified for product {ProductId}: CA${Old} → CA${New} ({Recipients} recipients)",
                productId, previousEffectivePrice, newEffectivePrice, rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Price-drop notification pipeline failed for product {ProductId}", productId);
        }
    }
}
