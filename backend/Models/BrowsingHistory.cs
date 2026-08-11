using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.Models;

/// <summary>
/// Records a buyer viewing a product on the storefront. Unique on
/// (UserId, ProductId) so a re-view simply refreshes ViewedAt rather
/// than appending — keeps the table small and the "recently viewed"
/// list naturally ordered.
/// </summary>
public class BrowsingHistory
{
    [Key]
    public int       BrowsingHistoryId { get; set; }
    public int?      UserId            { get; set; }
    public int       ProductId         { get; set; }
    public DateTime  ViewedAt          { get; set; } = DateTime.UtcNow;

    public User?   User    { get; set; }
    public Product Product { get; set; } = null!;
}
