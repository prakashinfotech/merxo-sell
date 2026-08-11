using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.Models;

/// <summary>
/// Append-only ledger of every status change made to an order.
/// One row per transition — never updated, never deleted — so we can replay
/// the full timeline for the buyer "Track Order" UI and admin audits.
/// </summary>
public class OrderStatusHistory
{
    [Key]
    public int       OrderStatusHistoryId { get; set; }
    public int       OrderId              { get; set; }

    [MaxLength(30)]
    public string    FromStatus           { get; set; } = string.Empty;

    [MaxLength(30)]
    public string    ToStatus             { get; set; } = string.Empty;

    [MaxLength(500)]
    public string?   Note                 { get; set; }

    /// <summary>UserId of the person who triggered the change (seller/admin/buyer).</summary>
    public int?      ChangedBy            { get; set; }

    [MaxLength(30)]
    public string?   ChangedByRole        { get; set; }

    public DateTime  CreatedAt            { get; set; } = DateTime.UtcNow;

    // Navigation
    public Order? Order    { get; set; }
    public User?  Changer  { get; set; }
}
