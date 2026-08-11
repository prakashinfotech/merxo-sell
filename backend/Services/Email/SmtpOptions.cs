namespace MerxoSell.API.Services.Email;

/// <summary>
/// Strongly-typed SMTP configuration bound from the <c>Smtp</c> section of
/// appsettings.json. <see cref="Enabled"/> is intentionally false by default
/// so a dev environment without real credentials never throws on send —
/// the email is logged and discarded instead.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host         { get; set; } = string.Empty;
    public int    Port         { get; set; } = 587;
    public string Username     { get; set; } = string.Empty;
    public string Password     { get; set; } = string.Empty;
    public bool   EnableSsl    { get; set; } = true;
    public string FromAddress  { get; set; } = "no-reply@localhost";
    public string FromName     { get; set; } = "MerxoSell";
    public bool   Enabled      { get; set; }
    public int    MaxRetries   { get; set; } = 3;
    public int    RetryDelayMs { get; set; } = 750;
}
