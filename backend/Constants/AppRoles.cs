namespace MerxoSell.API.Constants;

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Seller = "Seller";
    public const string Buyer = "Buyer";

    public static readonly string[] PublicRegistrationRoles = [Buyer, Seller];

    public static string NormalizePublicRole(string? role)
    {
        var normalized = role?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) ||
            normalized.Equals("Customer", StringComparison.OrdinalIgnoreCase))
        {
            return Buyer;
        }

        if (normalized.Equals(Buyer, StringComparison.OrdinalIgnoreCase))
            return Buyer;

        if (normalized.Equals(Seller, StringComparison.OrdinalIgnoreCase))
            return Seller;

        return normalized;
    }
}
