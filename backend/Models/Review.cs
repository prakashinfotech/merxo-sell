namespace MerxoSell.API.Models;

public class Review
{
    public int       ReviewId  { get; set; }
    public int       ProductId { get; set; }
    public int       UserId    { get; set; }
    public byte      Rating    { get; set; }
    public string?   Comment   { get; set; }
    /// <summary>Approved | Flagged | Rejected. Defaults to Approved (auto-publish).</summary>
    public string    Status    { get; set; } = "Approved";
    public string?   FlagReason { get; set; }
    public DateTime  CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public int?      ModeratedBy { get; set; }

    public Product Product { get; set; } = null!;
    public User    User    { get; set; } = null!;
}
