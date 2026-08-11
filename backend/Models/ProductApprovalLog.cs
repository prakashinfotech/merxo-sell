using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.Models;

public class ProductApprovalLog
{
    [Key]
    public int      LogId      { get; set; }
    public int      ProductId  { get; set; }
    public int?     ReviewedBy { get; set; }   // null = system action
    public string   OldStatus  { get; set; } = string.Empty;
    public string   NewStatus  { get; set; } = string.Empty;
    public string?  Note       { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    // Navigation
    public Product Product     { get; set; } = null!;
    public User?   Reviewer    { get; set; }
}
