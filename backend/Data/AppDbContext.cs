using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Models;

namespace MerxoSell.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role>               Roles               { get; set; }
    public DbSet<CurrencyRate>       CurrencyRates       { get; set; }
    public DbSet<User>               Users               { get; set; }
    public DbSet<Seller>             Sellers             { get; set; }
    public DbSet<Address>            Addresses           { get; set; }
    public DbSet<Category>           Categories          { get; set; }
    public DbSet<Manufacturer>       Manufacturers       { get; set; }
    public DbSet<Product>            Products            { get; set; }
    public DbSet<ProductColor>       ProductColors       { get; set; }
    public DbSet<ProductSize>        ProductSizes        { get; set; }
    public DbSet<ProductImage>       ProductImages       { get; set; }
    public DbSet<ProductVariant>     ProductVariants     { get; set; }
    public DbSet<ProductApprovalLog> ProductApprovalLogs { get; set; }
    public DbSet<ProductView>        ProductViews        { get; set; }
    public DbSet<Cart>               Carts               { get; set; }
    public DbSet<CartItem>           CartItems           { get; set; }
    public DbSet<Coupon>             Coupons             { get; set; }
    public DbSet<CouponUsageHistory> CouponUsageHistory  { get; set; }
    public DbSet<Order>              Orders              { get; set; }
    public DbSet<OrderItem>          OrderItems          { get; set; }
    public DbSet<Review>              Reviews              { get; set; }
    public DbSet<BrowsingHistory>     BrowsingHistories    { get; set; }
    public DbSet<OrderStatusHistory>  OrderStatusHistories { get; set; }
    public DbSet<UserPaymentMethod>   UserPaymentMethods   { get; set; }
    public DbSet<OfferBanner>         OfferBanners         { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── CurrencyRates ─────────────────────────────────────────────────
        modelBuilder.Entity<CurrencyRate>(e =>
        {
            e.HasKey(c => c.CurrencyCode);
            e.Property(c => c.CurrencyCode).HasMaxLength(10);
            e.Property(c => c.CurrencyName).HasMaxLength(100).IsRequired();
            e.Property(c => c.RateToCad).HasColumnType("decimal(18,6)").HasDefaultValue(1.000000m);
            e.Property(c => c.Symbol).HasMaxLength(10).HasDefaultValue("$");
        });

        // ── Users ─────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Role)
             .WithMany(r => r.Users)
             .HasForeignKey(u => u.RoleId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(u => u.PreferredCurrency).HasMaxLength(10).HasDefaultValue("CAD");
            e.HasOne(u => u.Currency)
             .WithMany(c => c.Users)
             .HasForeignKey(u => u.PreferredCurrency)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(u => u.IsDeleted);
        });

        // ── UserPaymentMethods ────────────────────────────────────────────
        modelBuilder.Entity<UserPaymentMethod>(e =>
        {
            e.HasIndex(pm => pm.UserId);
            e.HasIndex(pm => pm.IsDeleted);
            e.Property(pm => pm.Type).HasMaxLength(20).IsRequired();
            e.Property(pm => pm.Label).HasMaxLength(100).IsRequired();
            e.Property(pm => pm.SubLabel).HasMaxLength(100);

            e.HasOne(pm => pm.User)
             .WithMany(u => u.PaymentMethods)
             .HasForeignKey(pm => pm.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Sellers ───────────────────────────────────────────────────────
        modelBuilder.Entity<Seller>(e =>
        {
            e.HasIndex(s => s.UserId).IsUnique();
            e.Property(s => s.StoreName).HasMaxLength(200).IsRequired();
            e.Property(s => s.StoreDescription).HasMaxLength(1000);
            e.Property(s => s.ContactEmail).HasMaxLength(200);
            e.Property(s => s.Phone).HasMaxLength(50);

            e.HasOne(s => s.User)
             .WithOne(u => u.Seller)
             .HasForeignKey<Seller>(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(s => s.IsDeleted);
        });

        // ── Addresses ─────────────────────────────────────────────────────
        modelBuilder.Entity<Address>(e =>
        {
            e.Property(a => a.Label).HasMaxLength(50);
            e.HasIndex(a => a.UserId);
            e.HasIndex(a => new { a.UserId, a.IsDefault });
            e.HasOne(a => a.User)
             .WithMany(u => u.Addresses)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Categories (self-referencing) ─────────────────────────────────
        modelBuilder.Entity<Category>(e =>
        {
            e.HasOne(c => c.ParentCategory)
             .WithMany(c => c.SubCategories)
             .HasForeignKey(c => c.ParentCategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Manufacturers ─────────────────────────────────────────────────
        modelBuilder.Entity<Manufacturer>(e =>
        {
            e.Property(m => m.Name).HasMaxLength(200).IsRequired();
            e.Property(m => m.ContactEmail).HasMaxLength(200);
            e.Property(m => m.Phone).HasMaxLength(50);
            e.Property(m => m.Address).HasMaxLength(500);
            e.Property(m => m.Country).HasMaxLength(100);
            e.Property(m => m.Website).HasMaxLength(300);
        });

        // ── Products ──────────────────────────────────────────────────────
        // All prices stored in CAD; conversion is display-layer only.
        // Status defaults to Pending; only Approved+IsActive products are public.
        modelBuilder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Slug).IsUnique();
            e.HasIndex(p => p.Status);
            e.HasIndex(p => p.SellerId);
            e.HasIndex(p => p.CategoryId);
            e.HasIndex(p => p.Name);
            e.HasIndex(p => p.CreatedAt);
            e.HasIndex(p => new { p.Status, p.IsActive });
            e.HasIndex(p => new { p.Status, p.IsActive, p.IsDeleted });
            e.HasIndex(p => p.IsDeleted);

            e.Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
            e.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
            e.Property(p => p.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(p => p.ApprovalNote).HasMaxLength(1000);

            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Seller)
             .WithMany(s => s.Products)
             .HasForeignKey(p => p.SellerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Manufacturer)
             .WithMany(m => m.Products)
             .HasForeignKey(p => p.ManufacturerId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── ProductImages ─────────────────────────────────────────────────
        modelBuilder.Entity<ProductImage>(e =>
        {
            e.HasKey(pi => pi.ImageId);
            e.HasOne(pi => pi.Product)
             .WithMany(p => p.Images)
             .HasForeignKey(pi => pi.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ProductColors ─────────────────────────────────────────────────
        modelBuilder.Entity<ProductColor>(e =>
        {
            e.HasKey(c => c.ColorId);
            e.Property(c => c.ColorName).HasMaxLength(50).IsRequired();
            e.Property(c => c.HexCode).HasMaxLength(10);
            e.HasOne(c => c.Product).WithMany(p => p.Colors).HasForeignKey(c => c.ProductId);
        });

        // ── ProductSizes ──────────────────────────────────────────────────
        modelBuilder.Entity<ProductSize>(e =>
        {
            e.HasKey(s => s.SizeId);
            e.Property(s => s.SizeName).HasMaxLength(50).IsRequired();
            e.HasOne(s => s.Product).WithMany(p => p.Sizes).HasForeignKey(s => s.ProductId);
        });

        // ── ProductVariants ───────────────────────────────────────────────
        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.HasKey(pv => pv.VariantId);
            e.Property(v => v.PriceDelta).HasColumnType("decimal(18,2)");
            e.HasOne(v => v.Product)
             .WithMany(p => p.Variants)
             .HasForeignKey(v => v.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many relationship with Images
            e.HasMany(v => v.Images)
             .WithMany(i => i.Variants)
             .UsingEntity<Dictionary<string, object>>(
                 "ProductVariantImages",
                 r => r.HasOne<ProductImage>().WithMany().HasForeignKey("ImageId"),
                 l => l.HasOne<ProductVariant>().WithMany().HasForeignKey("VariantId"),
                 j =>
                 {
                     j.HasKey("VariantId", "ImageId");
                 });
        });

        // ── ProductApprovalLogs ───────────────────────────────────────────
        // Records every Pending→Approved / Pending→Rejected transition.
        modelBuilder.Entity<ProductApprovalLog>(e =>
        {
            e.HasIndex(l => l.ProductId);
            e.Property(l => l.OldStatus).HasMaxLength(20).IsRequired();
            e.Property(l => l.NewStatus).HasMaxLength(20).IsRequired();
            e.Property(l => l.Note).HasMaxLength(1000);

            e.HasOne(l => l.Product)
             .WithMany(p => p.ApprovalLogs)
             .HasForeignKey(l => l.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.Reviewer)
             .WithMany()
             .HasForeignKey(l => l.ReviewedBy)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── ProductViews ──────────────────────────────────────────────────
        modelBuilder.Entity<ProductView>(e =>
        {
            e.HasIndex(v => v.ProductId);
            e.HasIndex(v => v.ViewedAt);
            e.Property(v => v.IpAddress).HasMaxLength(45);

            e.HasOne(v => v.Product)
             .WithMany(p => p.Views)
             .HasForeignKey(v => v.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(v => v.User)
             .WithMany()
             .HasForeignKey(v => v.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Cart ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Cart>(e =>
        {
            e.HasIndex(c => c.UserId).IsUnique();
            e.HasOne(c => c.User)
             .WithOne(u => u.Cart)
             .HasForeignKey<Cart>(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CartItems ─────────────────────────────────────────────────────
        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasIndex(ci => new { ci.CartId, ci.ProductId, ci.VariantId }).IsUnique();
            e.HasOne(ci => ci.Cart)
             .WithMany(c => c.Items)
             .HasForeignKey(ci => ci.CartId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ci => ci.Product)
             .WithMany()
             .HasForeignKey(ci => ci.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ci => ci.Variant)
             .WithMany()
             .HasForeignKey(ci => ci.VariantId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Orders ────────────────────────────────────────────────────────
        // TotalAmountCAD = authoritative; CurrencyCode+DisplayTotal = display snapshot.
        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.TotalAmountCAD).HasColumnType("decimal(18,2)");
            e.Property(o => o.ShippingAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.DisplayTotal).HasColumnType("decimal(18,2)");
            e.Property(o => o.CurrencyCode).HasMaxLength(10);
            e.Property(o => o.CancellationReason).HasMaxLength(1000);
            e.HasIndex(o => o.Status);

            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Address)
             .WithMany()
             .HasForeignKey(o => o.AddressId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(o => o.Coupon)
             .WithMany(c => c.Orders)
             .HasForeignKey(o => o.CouponId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Coupons ───────────────────────────────────────────────────────
        modelBuilder.Entity<Coupon>(e =>
        {
            e.HasIndex(c => c.CouponCode).IsUnique();
            e.HasIndex(c => new { c.IsActive, c.StartDate, c.ExpiryDate });
            e.HasIndex(c => c.IsDeleted);
            e.Property(c => c.CouponCode).HasMaxLength(50).IsRequired();
            e.Property(c => c.Title).HasMaxLength(150).IsRequired();
            e.Property(c => c.Description).HasMaxLength(1000);
            e.Property(c => c.DiscountType).HasMaxLength(30).IsRequired();
            e.Property(c => c.DiscountValue).HasColumnType("decimal(18,2)");
            e.Property(c => c.MinimumPurchaseAmount).HasColumnType("decimal(18,2)");
            e.Property(c => c.MaximumDiscountAmount).HasColumnType("decimal(18,2)");
        });

        // ── CouponUsageHistory ────────────────────────────────────────────
        modelBuilder.Entity<CouponUsageHistory>(e =>
        {
            e.HasIndex(h => new { h.CouponId, h.UserId }).IsUnique();
            e.HasIndex(h => h.UserId);
            e.HasIndex(h => h.UsedAt);
            e.Property(h => h.OrderAmount).HasColumnType("decimal(18,2)");
            e.Property(h => h.DiscountAmount).HasColumnType("decimal(18,2)");
            e.HasOne(h => h.Coupon)
             .WithMany(c => c.UsageHistory)
             .HasForeignKey(h => h.CouponId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(h => h.User)
             .WithMany()
             .HasForeignKey(h => h.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(h => h.Order)
             .WithMany()
             .HasForeignKey(h => h.OrderId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── OrderItems ────────────────────────────────────────────────────
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(oi => oi.UnitPriceCAD).HasColumnType("decimal(18,2)");
            e.HasOne(oi => oi.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(oi => oi.Product)
             .WithMany()
             .HasForeignKey(oi => oi.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(oi => oi.Variant)
             .WithMany()
             .HasForeignKey(oi => oi.VariantId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Reviews ───────────────────────────────────────────────────────
        // One review per buyer per product — unique constraint enforced.
        modelBuilder.Entity<Review>(e =>
        {
            e.HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();
            e.HasIndex(r => r.Status);
            e.Property(r => r.Status).HasMaxLength(20).HasDefaultValue("Approved");
            e.Property(r => r.FlagReason).HasMaxLength(500);
            e.HasOne(r => r.Product)
             .WithMany(p => p.Reviews)
             .HasForeignKey(r => r.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.User)
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── BrowsingHistory ───────────────────────────────────────────────
        modelBuilder.Entity<BrowsingHistory>(e =>
        {
            e.HasIndex(b => new { b.UserId, b.ProductId })
             .IsUnique()
             .HasFilter("[UserId] IS NOT NULL");
            e.HasIndex(b => b.ViewedAt);
            e.HasOne(b => b.User)
             .WithMany()
             .HasForeignKey(b => b.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.Product)
             .WithMany()
             .HasForeignKey(b => b.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── OrderStatusHistory ────────────────────────────────────────────
        modelBuilder.Entity<OrderStatusHistory>(e =>
        {
            e.HasIndex(h => h.OrderId);
            e.HasIndex(h => h.CreatedAt);
            e.HasOne(h => h.Order)
             .WithMany(o => o.StatusHistory)
             .HasForeignKey(h => h.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(h => h.Changer)
             .WithMany()
             .HasForeignKey(h => h.ChangedBy)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── OfferBanners ──────────────────────────────────────────────────
        modelBuilder.Entity<OfferBanner>(e =>
        {
            e.HasKey(b => b.BannerId);

            e.HasIndex(b => new { b.Slot, b.IsActive, b.SortOrder });
            e.HasIndex(b => new { b.IsActive, b.StartsAt, b.EndsAt });

            e.Property(b => b.Slot).HasMaxLength(20).IsRequired().HasDefaultValue("Hero");
            e.ToTable(t => t.HasCheckConstraint("CK_OfferBanners_Slot",
                "[Slot] IN ('Hero','MidLeft','MidRight','Strip')"));

            e.Property(b => b.Title).HasMaxLength(200).IsRequired();
            e.Property(b => b.Subtitle).HasMaxLength(300);
            e.Property(b => b.BadgeText).HasMaxLength(40);
            e.Property(b => b.ImageUrl).HasMaxLength(500).IsRequired();
            e.Property(b => b.SideImageUrl).HasMaxLength(500);
            e.Property(b => b.CtaLabel).HasMaxLength(60);
            e.Property(b => b.CtaUrl).HasMaxLength(500);
            e.Property(b => b.SecondaryLabel).HasMaxLength(60);
            e.Property(b => b.SecondaryUrl).HasMaxLength(500);
            e.Property(b => b.BackgroundColor).HasMaxLength(20);
            e.Property(b => b.TextColor).HasMaxLength(20);
            e.Property(b => b.SortOrder).HasDefaultValue(0);

            e.HasOne(b => b.LinkedProduct)
             .WithMany()
             .HasForeignKey(b => b.LinkedProductId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.LinkedCategory)
             .WithMany()
             .HasForeignKey(b => b.LinkedCategoryId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
