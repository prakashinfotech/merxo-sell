using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.Models;

public class Seller
{
    [Key]
    public int      SellerId         { get; set; }
    public int      UserId           { get; set; }
    public string   StoreName        { get; set; } = string.Empty;
    public string?  StoreDescription { get; set; }
    public string?  ContactEmail     { get; set; }
    public string?  Phone            { get; set; }
    public bool     IsVerified       { get; set; } = false;
    public bool     IsActive         { get; set; } = true;
    public bool     IsDeleted        { get; set; } = false;
    public DateTime CreatedAt        { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt       { get; set; }

    // Navigation
    public User                       User     { get; set; } = null!;
    public ICollection<Product>       Products { get; set; } = [];
}
