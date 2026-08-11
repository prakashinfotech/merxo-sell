namespace MerxoSell.API.Models;

public class Product
{
    public int       ProductId    { get; set; }
    public int       CategoryId   { get; set; }
    public int       SellerId     { get; set; }
    public int?      ManufacturerId { get; set; }
    public string    Name         { get; set; } = string.Empty;
    public string    Slug         { get; set; } = string.Empty;
    public string?   Description  { get; set; }
    public decimal   BasePrice    { get; set; }
    public decimal?  SalePrice    { get; set; }
    public int       Stock        { get; set; }
    public string    Status       { get; set; } = "Pending";   // Pending | Approved | Rejected
    public string?   ApprovalNote { get; set; }
    public int       ViewCount    { get; set; } = 0;
    public bool      IsActive     { get; set; } = true;
    public bool      IsDeleted    { get; set; } = false;
    public DateTime  CreatedAt    { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt    { get; set; }

    // Navigation
    public Category               Category      { get; set; } = null!;
    public Seller                 Seller        { get; set; } = null!;
    public Manufacturer?          Manufacturer  { get; set; }
    public ICollection<ProductImage>   Images   { get; set; } = [];
    public ICollection<ProductColor>   Colors   { get; set; } = [];
    public ICollection<ProductSize>    Sizes    { get; set; } = [];
    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<Review>         Reviews  { get; set; } = [];
    public ICollection<ProductApprovalLog> ApprovalLogs { get; set; } = [];
    public ICollection<ProductView>    Views    { get; set; } = [];
}
