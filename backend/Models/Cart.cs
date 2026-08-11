namespace MerxoSell.API.Models;

public class Cart
{
    public int       CartId    { get; set; }
    public int       UserId    { get; set; }
    public DateTime  CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User                  User  { get; set; } = null!;
    public ICollection<CartItem> Items { get; set; } = [];
}
