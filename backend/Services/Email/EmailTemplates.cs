using System.Globalization;
using System.Text;
using System.IO;
using MerxoSell.API.DTOs.Products;

namespace MerxoSell.API.Services.Email;

/// <summary>
/// Handles email template loading and variable replacement.
/// Now uses physical HTML files from Services/Email/Templates/.
/// </summary>
internal static class EmailTemplates
{
    private static string? _layoutCache;
    private static readonly object _lock = new();

    private static string GetTemplateDir() 
    {
        var root = AppContext.BaseDirectory;
        // Search upwards for the Services folder if necessary, 
        // or assume standard deployment structure.
        return Path.Combine(root, "Services", "Email", "Templates");
    }

    private static string LoadTemplate(string relativePath)
    {
        var fullPath = Path.Combine(GetTemplateDir(), relativePath);
        if (!File.Exists(fullPath))
        {
            // Fallback for development if BaseDirectory is deep in bin/Debug
            var devPath = Path.Combine(Directory.GetCurrentDirectory(), "Services", "Email", "Templates", relativePath);
            if (File.Exists(devPath)) return File.ReadAllText(devPath);
            
            throw new FileNotFoundException($"Email template not found: {fullPath}");
        }
        return File.ReadAllText(fullPath);
    }

    private static string ResolveUrl(string? url, string apiBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";
        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;
        
        var baseUri = new Uri(apiBaseUrl.TrimEnd('/'));
        var relative = url.TrimStart('/');
        return new Uri(baseUri, relative).ToString();
    }

    private static string BuildHtml(string subject, string body, string appBaseUrl, string apiBaseUrl)
    {
        lock (_lock)
        {
            _layoutCache ??= LoadTemplate("Layout.html");
        }

        return _layoutCache
            .Replace("{{Subject}}", Html(subject))
            .Replace("{{Body}}", body)
            .Replace("{{AppUrl}}", appBaseUrl.TrimEnd('/'))
            .Replace("{{ApiUrl}}", apiBaseUrl.TrimEnd('/'))
            .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());
    }

    public static string Welcome(string fullName, IReadOnlyList<ProductListDto>? bestSellers, string appBaseUrl, string apiBaseUrl)
    {
        var recommendationsHtml = new StringBuilder();
        if (bestSellers != null && bestSellers.Count > 0)
        {
            recommendationsHtml.AppendLine(@"
                <tr>
                    <td>
                        <table width='100%' cellpadding='48' cellspacing='0' border='0' style='background-color: #f8fafc; border-radius: 32px; border: 1.5px solid #f1f5f9; text-align: center;'>
                            <tr>
                                <td>
                                    <h4 style='margin: 0 0 32px; font-size: 14px; font-weight: 800; color: #0f172a; text-transform: uppercase; letter-spacing: 2px;'>Recommended for You</h4>
                                    <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                                        <tr>");

            for (int i = 0; i < Math.Min(bestSellers.Count, 2); i++)
            {
                var p = bestSellers[i];
                if (i > 0) recommendationsHtml.AppendLine("<td width='4%'></td>");

                var pImg = ResolveUrl(p.PrimaryImageUrl, apiBaseUrl);
                recommendationsHtml.AppendLine($@"
                    <td width='48%'>
                        <table width='100%' cellpadding='24' cellspacing='0' border='0' style='background: #ffffff; border-radius: 24px; border: 1px solid #e2e8f0;'>
                            <tr>
                                <td align='center'>
                                    <img src='{Html(pImg)}' width='120' style='border-radius: 16px; margin-bottom: 16px;'>
                                    <div style='font-size: 15px; font-weight: 700; color: #0f172a;'>{Html(p.Name)}</div>
                                    <div style='font-size: 13px; color: #94a3b8; margin-top: 4px;'>CA${p.BasePrice:0.00}</div>
                                </td>
                            </tr>
                        </table>
                    </td>");
            }

            recommendationsHtml.AppendLine(@"
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>");
        }

        var body = LoadTemplate("Buyer/Welcome.html")
            .Replace("{{FullName}}", Html(fullName))
            .Replace("{{RecommendationsHtml}}", recommendationsHtml.ToString())
            .Replace("{{AppUrl}}", appBaseUrl.TrimEnd('/'))
            .Replace("{{ApiUrl}}", apiBaseUrl.TrimEnd('/'));

        return BuildHtml("Welcome to MerxoSell", body, appBaseUrl, apiBaseUrl);
    }

    public static string OrderConfirmation(
        string fullName, int orderId, IReadOnlyList<OrderEmailLine> lines,
        decimal totalAmountCAD, string currencyCode, decimal displayTotal,
        string status, DateTime estimatedDeliveryUtc, string appBaseUrl, string apiBaseUrl)
    {
        var itemsHtml = new StringBuilder();
        foreach (var l in lines)
        {
            var lineTotal = (l.UnitPriceCAD * l.Quantity).ToString("0.00", CultureInfo.InvariantCulture);
            var imageUrl = ResolveUrl(l.ProductImageUrl, apiBaseUrl);
            if (string.IsNullOrWhiteSpace(imageUrl))
                imageUrl = "https://img.icons8.com/ios-filled/50/ff6000/box.png";

            itemsHtml.AppendLine($@"
                <tr>
                    <td style='padding: 24px 0; border-bottom: 1.5px solid #f1f5f9;'>
                        <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                            <tr>
                                <td width='64' valign='top' style='padding-right: 16px;'>
                                    <img src='{Html(imageUrl)}' width='64' style='border-radius: 12px; border: 1px solid #f1f5f9;'>
                                </td>
                                <td valign='top'>
                                    <div style='font-size: 16px; font-weight: 800; color: #0f172a; margin-bottom: 4px;'>{Html(l.ProductName)}</div>
                                    {(string.IsNullOrEmpty(l.VariantInfo) ? "" : $"<div style='font-size: 14px; color: #64748b; margin-bottom: 6px; font-weight: 500;'>{Html(l.VariantInfo)}</div>")}
                                    <div style='font-size: 14px; color: #94a3b8; font-weight: 700;'>QUANTITY: {l.Quantity}</div>
                                </td>
                                <td valign='top' align='right' style='font-size: 16px; font-weight: 900; color: #0f172a;'>
                                    CA${lineTotal}
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>");
        }

        var body = LoadTemplate("Buyer/OrderConfirmation.html")
            .Replace("{{FullName}}", Html(fullName))
            .Replace("{{OrderId}}", orderId.ToString())
            .Replace("{{Date}}", DateTime.UtcNow.ToString("MMM d, yyyy"))
            .Replace("{{Status}}", status)
            .Replace("{{Address}}", "Your verified shipping address")
            .Replace("{{ItemsHtml}}", itemsHtml.ToString())
            .Replace("{{Subtotal}}", $"CA${totalAmountCAD.ToString("0.00", CultureInfo.InvariantCulture)}")
            .Replace("{{Total}}", $"{currencyCode} {displayTotal.ToString("0.00", CultureInfo.InvariantCulture)}")
            .Replace("{{AppUrl}}", appBaseUrl.TrimEnd('/'))
            .Replace("{{ApiUrl}}", apiBaseUrl.TrimEnd('/'));

        return BuildHtml($"Order #{orderId} Confirmed", body, appBaseUrl, apiBaseUrl);
    }

    public static string OrderStatusUpdate(
        string fullName, int orderId, string previousStatus, string newStatus,
        string? note, string appBaseUrl, string apiBaseUrl)
    {
        var body = LoadTemplate("Buyer/StatusUpdate.html")
            .Replace("{{FullName}}", Html(fullName))
            .Replace("{{OrderId}}", orderId.ToString())
            .Replace("{{NewStatus}}", newStatus)
            .Replace("{{NoteHtml}}", string.IsNullOrWhiteSpace(note) ? "" : $@"
                <div style='margin-top: 32px; padding: 20px; background: #fff; border-radius: 16px; border: 1px solid #e2e8f0;'>
                    <div style='font-size: 11px; font-weight: 800; color: #94a3b8; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 8px;'>Special Note</div>
                    <div style='font-size: 14px; color: #475569; font-weight: 500; line-height: 1.6;'>{Html(note)}</div>
                </div>")
            .Replace("{{AppUrl}}", appBaseUrl.TrimEnd('/'))
            .Replace("{{ApiUrl}}", apiBaseUrl.TrimEnd('/'));

        return BuildHtml($"Logistics Update: Order #{orderId}", body, appBaseUrl, apiBaseUrl);
    }

    public static string PriceDrop(
        string fullName, string productName, string? productImageUrl,
        decimal oldPriceCAD, decimal newPriceCAD,
        string currencyCode, decimal oldPriceDisplay, decimal newPriceDisplay,
        string appBaseUrl, string apiBaseUrl)
    {
        var discount = oldPriceCAD - newPriceCAD;
        var percent = oldPriceCAD > 0 ? (discount / oldPriceCAD) * 100m : 0m;

        var absProductImg = ResolveUrl(productImageUrl, apiBaseUrl);
        var imageHtml = string.IsNullOrWhiteSpace(absProductImg)
            ? ""
            : $@"<tr>
                    <td align='center' style='padding-bottom: 40px;'>
                        <div style='width: 240px; height: 240px; background: #f8fafc; border-radius: 40px; border: 1.5px solid #f1f5f9; display: flex; align-items: center; justify-content: center; overflow: hidden;'>
                            <img src='{Html(absProductImg)}' width='200' style='display: block;' alt='Product'>
                        </div>
                    </td>
                </tr>";

        var body = LoadTemplate("Buyer/PriceDrop.html")
            .Replace("{{FullName}}", Html(fullName))
            .Replace("{{ProductName}}", Html(productName))
            .Replace("{{ProductImageHtml}}", imageHtml)
            .Replace("{{OldPrice}}", oldPriceDisplay.ToString("0.00", CultureInfo.InvariantCulture))
            .Replace("{{NewPrice}}", newPriceDisplay.ToString("0.00", CultureInfo.InvariantCulture))
            .Replace("{{Percent}}", percent.ToString("0", CultureInfo.InvariantCulture))
            .Replace("{{AppUrl}}", appBaseUrl.TrimEnd('/'))
            .Replace("{{ApiUrl}}", apiBaseUrl.TrimEnd('/'));

        return BuildHtml($"Price drop on {productName}", body, appBaseUrl, apiBaseUrl);
    }

    public static string AccountStatusNotification(string fullName, bool isActive, bool isDeleted, string appBaseUrl, string apiBaseUrl)
    {
        string action;
        string message;

        if (isDeleted)
        {
            action = "Account Deleted";
            message = "Your account has been permanently removed from our system. This action is irreversible.";
        }
        else if (!isActive)
        {
            action = "Account Deactivated";
            message = "Your account has been temporarily deactivated. You will no longer be able to log in until it is reactivated.";
        }
        else
        {
            action = "Account Reactivated";
            message = "Great news! Your account has been reactivated. You can now log back into the marketplace.";
        }

        var body = LoadTemplate("Account/StatusNotification.html")
            .Replace("{{FullName}}", Html(fullName))
            .Replace("{{StatusAction}}", action)
            .Replace("{{StatusMessage}}", message)
            .Replace("{{AppUrl}}", appBaseUrl.TrimEnd('/'))
            .Replace("{{ApiUrl}}", apiBaseUrl.TrimEnd('/'));

        return BuildHtml($"Account Update: {action}", body, appBaseUrl, apiBaseUrl);
    }

    private static string Html(string value) => System.Net.WebUtility.HtmlEncode(value ?? "");
}
