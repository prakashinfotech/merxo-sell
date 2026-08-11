namespace MerxoSell.API.Models;

public class Address
{
    public int       AddressId    { get; set; }
    public int       UserId       { get; set; }
    public string    FullName     { get; set; } = string.Empty;
    public string    Phone        { get; set; } = string.Empty;
    public string    AddressLine1 { get; set; } = string.Empty;
    public string?   AddressLine2 { get; set; }
    public string    City         { get; set; } = string.Empty;
    public string?   State        { get; set; }
    public string    PostalCode   { get; set; } = string.Empty;
    public string    Country      { get; set; } = "US";
    public string?   Label        { get; set; }   // e.g. "Home", "Work"
    public bool      IsDefault    { get; set; }
    public DateTime  CreatedAt    { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
