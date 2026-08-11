using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.Models;

/// <summary>
/// Stores saved payment methods for a user (Card or UPI).
/// Used in the checkout flow to streamline the purchase experience.
/// </summary>
public class UserPaymentMethod
{
    [Key]
    public int      Id          { get; set; }
    public int      UserId      { get; set; }
    
    /// <summary>
    /// Type of payment method: "Card" or "UPI".
    /// </summary>
    public string   Type        { get; set; } = "Card"; 
    
    /// <summary>
    /// Friendly label (e.g., "Visa ....4242" or "harsh@upi").
    /// </summary>
    public string   Label       { get; set; } = string.Empty;
    
    /// <summary>
    /// Sub-label for extra info (e.g., "Expires 12/26").
    /// </summary>
    public string?  SubLabel    { get; set; }
    
    public bool     IsDefault   { get; set; } = false;
    public bool     IsDeleted   { get; set; } = false;
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}
