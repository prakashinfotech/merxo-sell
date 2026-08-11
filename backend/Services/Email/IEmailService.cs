namespace MerxoSell.API.Services.Email;
using MerxoSell.API.DTOs.Products;

/// <summary>
/// Centralised email facade. Implementations must be safe to inject into any
/// scope and never throw on a failure (return false). Hard-failing on a
/// transactional event would block business-critical writes.
/// </summary>
public interface IEmailService
{
    /// <summary>Generic raw send — used by the higher-level helpers.</summary>
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody, string? textBody = null);

    Task<bool> SendWelcomeAsync(string toEmail, string fullName, IReadOnlyList<ProductListDto>? bestSellers = null);

    Task<bool> SendOrderConfirmationAsync(
        string   toEmail,
        string   fullName,
        int      orderId,
        IReadOnlyList<OrderEmailLine> lines,
        decimal  totalAmountCAD,
        string   currencyCode,
        decimal  displayTotal,
        string   status,
        DateTime estimatedDeliveryUtc);

    Task<bool> SendOrderStatusUpdateAsync(
        string toEmail, string fullName,
        int orderId, string previousStatus, string newStatus,
        string? note);

    Task<bool> SendPriceDropAsync(
        string  toEmail,
        string  fullName,
        string  productName,
        string? productImageUrl,
        decimal oldPriceCAD,
        decimal newPriceCAD,
        string  currencyCode,
        decimal oldPriceDisplay,
        decimal newPriceDisplay);

    Task<bool> SendAccountStatusNotificationAsync(string toEmail, string fullName, bool isActive, bool isDeleted);
}

/// <summary>Compact view of an order line used inside the order-email body.</summary>
public sealed record OrderEmailLine(
    string  ProductName,
    int     Quantity,
    decimal UnitPriceCAD,
    string? VariantInfo,
    string? ProductImageUrl);
