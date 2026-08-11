namespace MerxoSell.API.Models;

public class User
{
    public int       UserId            { get; set; }
    public int       RoleId            { get; set; }
    public string    FullName          { get; set; } = string.Empty;
    public string    Email             { get; set; } = string.Empty;
    public string    PasswordHash      { get; set; } = string.Empty;
    public string?   Phone             { get; set; }
    public string    PreferredCurrency { get; set; } = "CAD";
    public string    Country           { get; set; } = "Canada";
    public bool      IsActive          { get; set; } = true;
    public bool      IsDeleted         { get; set; } = false;
    public DateTime  CreatedAt         { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt         { get; set; }

    // Navigation
    public Role                          Role           { get; set; } = null!;
    public CurrencyRate                  Currency       { get; set; } = null!;
    public Cart?                         Cart           { get; set; }
    public Seller?                       Seller         { get; set; }   // null unless role=Seller
    public ICollection<Address>          Addresses      { get; set; } = [];
    public ICollection<Order>            Orders         { get; set; } = [];
    public ICollection<UserPaymentMethod> PaymentMethods { get; set; } = [];
}
