using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using MerxoSell.API.DTOs.Products;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services.Email;

/// <summary>
/// SMTP-backed implementation of <see cref="IEmailService"/>.
/// Uses <see cref="System.Net.Mail.SmtpClient"/> (no extra NuGet) and exposes
/// retry-safe sending with structured logging. When <c>Smtp:Enabled=false</c>
/// the body is logged and discarded so dev environments don't fail.
/// </summary>
public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions               _options;
    private readonly IConfiguration            _config;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly IServiceScopeFactory      _scopeFactory;

    public SmtpEmailService(
        IOptions<SmtpOptions> options,
        IConfiguration config,
        ILogger<SmtpEmailService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _options      = options.Value;
        _config       = config;
        _logger       = logger;
        _scopeFactory = scopeFactory;
    }

    private string AppBaseUrl => _config["AppBaseUrl"] ?? "http://localhost:4200";
    private string ApiBaseUrl => _config["ApiBaseUrl"] ?? "http://localhost:5000";

    // ── High-level helpers ────────────────────────────────────────────────
    public async Task<bool> SendWelcomeAsync(string toEmail, string fullName, IReadOnlyList<ProductListDto>? bestSellers = null)
    {
        // If bestSellers weren't passed in, try to fetch some to make the email look great
        if (bestSellers == null || bestSellers.Count == 0)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                bestSellers = await productService.GetBestSellersAsync(2);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch best sellers for welcome email to {To}", toEmail);
            }
        }

        return await SendAsync(toEmail,
            subject:  "Welcome to MerxoSell",
            htmlBody: EmailTemplates.Welcome(fullName, bestSellers, AppBaseUrl, ApiBaseUrl),
            textBody: $"Welcome to MerxoSell, {fullName}!\n\nWe're thrilled to have you join our community. Discover millions of products at unbeatable prices.\n\nStart shopping now: {AppBaseUrl}\n\n© {DateTime.UtcNow.Year} MerxoSell Marketplace");
    }

    public Task<bool> SendOrderConfirmationAsync(
        string toEmail, string fullName, int orderId,
        IReadOnlyList<OrderEmailLine> lines,
        decimal totalAmountCAD, string currencyCode, decimal displayTotal,
        string status, DateTime estimatedDeliveryUtc)
        => SendAsync(toEmail,
            subject:  $"Order #{orderId} Confirmed",
            htmlBody: EmailTemplates.OrderConfirmation(
                fullName, orderId, lines, totalAmountCAD, currencyCode, displayTotal,
                status, estimatedDeliveryUtc, AppBaseUrl, ApiBaseUrl),
            textBody: $"Order #{orderId} confirmed for {fullName}. Total: {currencyCode} {displayTotal:0.00}. Estimated delivery: {estimatedDeliveryUtc:MMM d}.");

    public Task<bool> SendOrderStatusUpdateAsync(
        string toEmail, string fullName,
        int orderId, string previousStatus, string newStatus,
        string? note)
        => SendAsync(toEmail,
            subject:  $"Status Update: Order #{orderId}",
            htmlBody: EmailTemplates.OrderStatusUpdate(
                fullName, orderId, previousStatus, newStatus, note, AppBaseUrl, ApiBaseUrl),
            textBody: $"Order #{orderId} updated to {newStatus}. Note: {note}");

    public Task<bool> SendPriceDropAsync(
        string toEmail, string fullName, string productName, string? productImageUrl,
        decimal oldPriceCAD, decimal newPriceCAD,
        string currencyCode, decimal oldPriceDisplay, decimal newPriceDisplay)
        => SendAsync(toEmail,
            subject:  $"Price Drop: {productName}",
            htmlBody: EmailTemplates.PriceDrop(
                fullName, productName, productImageUrl, oldPriceCAD, newPriceCAD,
                currencyCode, oldPriceDisplay, newPriceDisplay, AppBaseUrl, ApiBaseUrl),
            textBody: $"The price of {productName} has dropped to {currencyCode} {newPriceDisplay:0.00}!");

    public Task<bool> SendAccountStatusNotificationAsync(string toEmail, string fullName, bool isActive, bool isDeleted)
        => SendAsync(toEmail,
            subject:  "Account Status Update",
            htmlBody: EmailTemplates.AccountStatusNotification(fullName, isActive, isDeleted, AppBaseUrl, ApiBaseUrl));

    // ── Core send + retry ─────────────────────────────────────────────────
    public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody, string? textBody = null)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Email skipped — recipient address empty (subject: {Subject})", subject);
            return false;
        }

        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "SMTP disabled — would have sent '{Subject}' to {To}. Body length: {Length}",
                subject, toEmail, htmlBody?.Length ?? 0);
            return true; // treated as success so callers don't error in dev
        }

        var maxAttempts = Math.Max(1, _options.MaxRetries);
        var delayMs     = Math.Max(0, _options.RetryDelayMs);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var message = BuildMessage(toEmail, subject, htmlBody!, textBody);
                using var client  = BuildClient();

                await client.SendMailAsync(message);
                _logger.LogInformation(
                    "Email sent to {To} (subject: {Subject}, attempt {Attempt}/{Max}, host: {Host}:{Port}, ssl: {Ssl})",
                    toEmail, subject, attempt, maxAttempts,
                    _options.Host, _options.Port, _options.EnableSsl);
                return true;
            }
            catch (SmtpException ex) when (attempt < maxAttempts && IsTransient(ex))
            {
                _logger.LogWarning(ex,
                    "SMTP attempt {Attempt}/{Max} failed for {To} (subject: {Subject}, status: {Status}). Retrying in {Delay}ms.",
                    attempt, maxAttempts, toEmail, subject, ex.StatusCode, delayMs);
                if (delayMs > 0) await Task.Delay(delayMs * attempt);
            }
            catch (SmtpException ex)
            {
                // Non-transient SMTP error (most commonly authentication failure
                // — Gmail rejects regular passwords; an App Password is required).
                _logger.LogError(ex,
                    "SMTP send failed permanently for {To} (subject: {Subject}, status: {Status}, host: {Host}). " +
                    "Most common cause: Gmail/Outlook requires an *App Password* (not the account password). " +
                    "Generate one at https://myaccount.google.com/apppasswords and set it in appsettings.json → Smtp.Password.",
                    toEmail, subject, ex.StatusCode, _options.Host);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Email send failed permanently for {To} (subject: {Subject}, attempt {Attempt}/{Max}).",
                    toEmail, subject, attempt, maxAttempts);
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Auth failures (550, 535, etc.) are not worth retrying — they will keep
    /// failing until the password is fixed. Only retry network/timeout class
    /// errors where a quick re-attempt might succeed.
    /// </summary>
    private static bool IsTransient(SmtpException ex) => ex.StatusCode is
        SmtpStatusCode.ServiceNotAvailable or
        SmtpStatusCode.MailboxBusy or
        SmtpStatusCode.MailboxUnavailable or
        SmtpStatusCode.TransactionFailed or
        SmtpStatusCode.GeneralFailure;

    private MailMessage BuildMessage(string toEmail, string subject, string htmlBody, string? textBody)
    {
        var msg = new MailMessage
        {
            From            = new MailAddress(_options.FromAddress, _options.FromName),
            Subject         = subject,
            SubjectEncoding = Encoding.UTF8,
        };
        msg.To.Add(new MailAddress(toEmail));

        // Adding AlternateViews is the most robust way to ensure multipart emails 
        // (Text + HTML) are rendered correctly by all clients.
        
        if (!string.IsNullOrWhiteSpace(textBody))
        {
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                textBody, Encoding.UTF8, "text/plain"));
        }

        // --- CID Embedding Logic (Generic Fix for localhost images) ---
        // We scan for images using the ApiUrl/uploads/ pattern and embed them as LinkedResources.
        var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
        
        try
        {
            var apiUrl = ApiBaseUrl.TrimEnd('/');
            // Look for patterns like src="http://localhost:5000/uploads/drone_premium.png"
            var pattern = $@"{apiUrl}/uploads/([^""'\s>]+)";
            var matches = System.Text.RegularExpressions.Regex.Matches(htmlBody, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var embeddedCids = new HashSet<string>();

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var fullUrl = match.Value;
                var fileName = match.Groups[1].Value;
                var filePath = Path.Combine(wwwroot, "uploads", fileName);

                if (File.Exists(filePath) && !embeddedCids.Contains(fileName))
                {
                    var res = new LinkedResource(filePath)
                    {
                        ContentId = fileName,
                        ContentType = new System.Net.Mime.ContentType(GetMimeType(fileName))
                    };
                    htmlView.LinkedResources.Add(res);
                    
                    // Update the HTML to use the CID
                    htmlBody = htmlBody.Replace(fullUrl, "cid:" + fileName);
                    embeddedCids.Add(fileName);
                }
            }

            // Re-create the HTML view if we made replacements
            if (embeddedCids.Count > 0)
            {
                htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
                foreach (var cid in embeddedCids)
                {
                    var filePath = Path.Combine(wwwroot, "uploads", cid);
                    var res = new LinkedResource(filePath)
                    {
                        ContentId = cid,
                        ContentType = new System.Net.Mime.ContentType(GetMimeType(cid))
                    };
                    htmlView.LinkedResources.Add(res);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to embed images as CIDs. Falling back to external URLs.");
        }

        msg.AlternateViews.Add(htmlView);
        return msg;
    }

    private static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private SmtpClient BuildClient()
    {
        var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl             = _options.EnableSsl,
            DeliveryMethod        = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials           = new NetworkCredential(_options.Username, _options.Password),
            Timeout               = 30_000,
        };
        return client;
    }
}
