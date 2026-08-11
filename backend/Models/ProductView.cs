using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.Models;

public class ProductView
{
    [Key]
    public int      ViewId    { get; set; }
    public int      ProductId { get; set; }
    public int?     UserId    { get; set; }
    public DateTime ViewedAt  { get; set; } = DateTime.UtcNow;
    public string?  IpAddress { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public User?   User    { get; set; }
}
