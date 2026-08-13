using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MerxoSell.API.Data;
using MerxoSell.API.DTOs.Orders;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Email;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class OrderService : IOrderService
{
    /// <summary>Allowed status values — kept here so both Update and Cancel share a single source of truth.</summary>
    public static readonly string[] AllowedStatuses =
    {
        "Pending", "Processing", "Shipped", "Delivered", "Cancelled",
    };

    private readonly IOrderRepository                _orderRepo;
    private readonly ICartRepository                 _cartRepo;
    private readonly IOrderStatusHistoryRepository   _historyRepo;
    private readonly ICouponService                  _couponService;
    private readonly IEmailService                   _email;
    private readonly AppDbContext                    _db;
    private readonly IServiceScopeFactory            _scopeFactory;
    private readonly ILogger<OrderService>           _logger;

    public OrderService(
        IOrderRepository orderRepo,
        ICartRepository cartRepo,
        IOrderStatusHistoryRepository historyRepo,
        ICouponService couponService,
        IEmailService email,
        AppDbContext db,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderService> logger)
    {
        _orderRepo    = orderRepo;
        _cartRepo     = cartRepo;
        _historyRepo  = historyRepo;
        _couponService = couponService;
        _email        = email;
        _db           = db;
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    // ── Create ────────────────────────────────────────────────────────────
    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        // Prefer the explicit Items the buyer sent (the source of truth on
        // the checkout page). Fall back to the server-side cart so callers
        // that only send totals still work.
        var explicitItems = dto.Items?
            .Where(i => i is { ProductId: > 0, Quantity: > 0 })
            .ToList();

        // We still load the cart so we can clear it after a successful order.
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);

        // Build the line-up source: explicit items if any, else the cart.
        var lineSource = explicitItems is { Count: > 0 }
            ? explicitItems.Select(i => (i.ProductId, i.VariantId, i.Quantity)).ToList()
            : cart.Items.Select(ci => (ci.ProductId, ci.VariantId, ci.Quantity)).ToList();

        if (lineSource.Count == 0)
            throw new InvalidOperationException(
                "Cart is empty. Add at least one product before placing an order.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        int newOrderId;
        try
        {
            decimal totalCad = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in lineSource)
            {
                var product = await _db.Products.FindAsync(item.ProductId)
                    ?? throw new InvalidOperationException($"Product {item.ProductId} not found.");

                decimal price = product.SalePrice ?? product.BasePrice;

                if (item.VariantId.HasValue)
                {
                    var variant = await _db.ProductVariants.FindAsync(item.VariantId.Value)
                        ?? throw new InvalidOperationException($"Variant {item.VariantId} not found.");

                    if (variant.Stock < item.Quantity)
                        throw new InvalidOperationException($"Not enough stock for variant {variant.VariantId}.");

                    variant.Stock -= item.Quantity;
                    price += variant.PriceDelta;
                }
                else
                {
                    if (product.Stock < item.Quantity)
                        throw new InvalidOperationException($"Not enough stock for product {product.Name}.");
                    product.Stock -= item.Quantity;
                }

                totalCad += price * item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId    = item.ProductId,
                    VariantId    = item.VariantId,
                    ProductName  = product.Name,
                    Quantity     = item.Quantity,
                    UnitPriceCAD = price,
                });
            }

            decimal discountAmount = 0;
            string? normalizedCouponCode = null;
            int? couponId = null;
            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var couponResult = await _couponService.ValidateAsync(dto.CouponCode, totalCad, userId);
                if (!couponResult.IsValid)
                    throw new InvalidOperationException(couponResult.Message ?? "Coupon is invalid.");

                discountAmount = couponResult.DiscountAmount;
                normalizedCouponCode = couponResult.CouponCode;
                couponId = await _db.Coupons
                    .Where(c => c.CouponCode == normalizedCouponCode)
                    .Select(c => (int?)c.CouponId)
                    .FirstOrDefaultAsync();
            }

            var payableCad = Math.Max(0, totalCad + dto.ShippingAmount - discountAmount);

            var order = new Order
            {
                UserId         = userId,
                AddressId      = dto.AddressId,
                TotalAmountCAD = totalCad,
                ShippingAmount = dto.ShippingAmount,
                DiscountAmount = discountAmount,
                CouponId       = couponId,
                CouponCode     = normalizedCouponCode,
                DisplayTotal   = payableCad,
                CurrencyCode   = dto.CurrencyCode,
                PaymentMethod  = dto.PaymentMethod ?? "Razorpay",
                PaymentTransactionId = dto.PaymentTransactionId,
                Status         = "Pending",
                Items          = orderItems,
            };

            // Each repo method calls SaveChangesAsync() internally — do NOT call it
            // again here; doing so causes EF to attempt a redundant flush which can
            // leave the SqlTransaction in a "completed" state before CommitAsync.
            await _orderRepo.CreateOrderAsync(order);
            if (!string.IsNullOrWhiteSpace(normalizedCouponCode) && discountAmount > 0)
            {
                await _couponService.RecordUsageAsync(
                    normalizedCouponCode, userId, order.OrderId, totalCad, discountAmount);
            }
            await _cartRepo.ClearCartAsync(cart.CartId);

            // Seed status history with the initial "creation" row so the
            // tracking timeline always has at least one entry.
            await _historyRepo.RecordAsync(new OrderStatusHistory
            {
                OrderId       = order.OrderId,
                FromStatus    = "(new)",
                ToStatus      = "Pending",
                Note          = "Order placed.",
                ChangedBy     = userId,
                ChangedByRole = "Buyer",
            });

            await tx.CommitAsync();
            newOrderId = order.OrderId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
        // ── Transaction is committed AND disposed here (await using scope closed). ──
        // Now it is safe to query the DB; the connection no longer has a stale
        // SqlTransaction reference that would trigger "has completed" errors.

        // Send the confirmation email out-of-band; failure must not roll the order back.
        _ = SendOrderConfirmationSafelyAsync(newOrderId);

        var created = await _orderRepo.GetByIdAsync(newOrderId);
        return MapToDto(created!);
    }

    // ── Reads ─────────────────────────────────────────────────────────────
    public async Task<OrderDto?> GetOrderAsync(int orderId, int? userId = null)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, userId);
        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
        => (await _orderRepo.GetUserOrdersAsync(userId)).Select(MapToDto);

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        => (await _orderRepo.GetAllOrdersAsync()).Select(MapToDto);

    // ── Mutations ─────────────────────────────────────────────────────────
    public async Task UpdateOrderStatusAsync(
        int orderId, string status, string? note = null,
        int? changedByUserId = null, string? changedByRole = null)
    {
        if (!AllowedStatuses.Contains(status))
            throw new InvalidOperationException($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");

        var order = await _db.Orders.Include(o => o.User)
            .FirstOrDefaultAsync(o => o.OrderId == orderId)
            ?? throw new InvalidOperationException($"Order {orderId} not found.");

        if (string.Equals(order.Status, status, StringComparison.OrdinalIgnoreCase)) return;
        if (order.Status is "Delivered" or "Refunded" && status != "Refunded")
            throw new InvalidOperationException($"Order is already {order.Status} and cannot move backwards.");

        var fromStatus = order.Status;
        order.Status    = status;
        order.UpdatedAt = DateTime.UtcNow;

        if (changedByUserId is not null && order.ApprovedBy is null && status != "Pending")
            order.ApprovedBy = changedByUserId;

        await _db.SaveChangesAsync();

        await _historyRepo.RecordAsync(new OrderStatusHistory
        {
            OrderId       = orderId,
            FromStatus    = fromStatus,
            ToStatus      = status,
            Note          = note,
            ChangedBy     = changedByUserId,
            ChangedByRole = changedByRole,
        });

        // Best-effort buyer notification — fire-and-forget so SMTP latency
        // does not block the admin HTTP response.
        if (!string.IsNullOrWhiteSpace(order.User?.Email))
            _ = FireStatusEmailAsync(order.User!.Email, order.User.FullName,
                                     orderId, fromStatus, status, note);
    }

    public async Task CancelOrderAsync(
        int orderId, string cancellationReason,
        int? changedByUserId = null, string? changedByRole = null)
    {
        if (string.IsNullOrWhiteSpace(cancellationReason))
            throw new InvalidOperationException("A cancellation reason is required.");

        var order = await _db.Orders.Include(o => o.User)
            .FirstOrDefaultAsync(o => o.OrderId == orderId)
            ?? throw new InvalidOperationException($"Order {orderId} not found.");

        if (order.Status is "Delivered" or "Refunded")
            throw new InvalidOperationException($"Order {orderId} is {order.Status} and cannot be cancelled.");

        var fromStatus = order.Status;
        order.Status              = "Cancelled";
        order.CancellationReason  = cancellationReason.Trim();
        order.UpdatedAt           = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _historyRepo.RecordAsync(new OrderStatusHistory
        {
            OrderId       = orderId,
            FromStatus    = fromStatus,
            ToStatus      = "Cancelled",
            Note          = cancellationReason.Trim(),
            ChangedBy     = changedByUserId,
            ChangedByRole = changedByRole,
        });

        if (!string.IsNullOrWhiteSpace(order.User?.Email))
            _ = FireStatusEmailAsync(order.User!.Email, order.User.FullName,
                                     orderId, fromStatus, "Cancelled", cancellationReason.Trim());
    }

    public async Task<IEnumerable<OrderStatusHistoryDto>> GetStatusHistoryAsync(int orderId)
    {
        var rows = await _historyRepo.GetForOrderAsync(orderId);
        return rows.Select(h => new OrderStatusHistoryDto(
            h.OrderStatusHistoryId, h.OrderId,
            h.FromStatus, h.ToStatus, h.Note,
            h.ChangedBy, h.Changer?.FullName, h.ChangedByRole,
            h.CreatedAt));
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private async Task FireStatusEmailAsync(
        string email, string name,
        int orderId, string fromStatus, string toStatus, string? note)
    {
        try
        {
            using var scope   = _scopeFactory.CreateScope();
            var emailer = scope.ServiceProvider.GetRequiredService<IEmailService>();
            await emailer.SendOrderStatusUpdateAsync(email, name, orderId, fromStatus, toStatus, note);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send status-update email for order {OrderId}", orderId);
        }
    }

    private async Task SendOrderConfirmationSafelyAsync(int orderId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var sp      = scope.ServiceProvider;
            var db      = sp.GetRequiredService<AppDbContext>();
            var repo    = sp.GetRequiredService<IOrderRepository>();
            var emailer = sp.GetRequiredService<IEmailService>();

            var order = await repo.GetByIdAsync(orderId);
            if (order is null) return;

            var user = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == order.UserId);
            if (user is null || string.IsNullOrWhiteSpace(user.Email)) return;

            var lines = order.Items.Select(i => new OrderEmailLine(
                ProductName:     i.Product?.Name ?? i.ProductName,
                Quantity:        i.Quantity,
                UnitPriceCAD:    i.UnitPriceCAD,
                VariantInfo:     BuildVariantInfo(i.Variant),
                ProductImageUrl: i.Product?.Images.FirstOrDefault()?.ImageUrl)).ToList();

            await emailer.SendOrderConfirmationAsync(
                toEmail:              user.Email,
                fullName:             user.FullName,
                orderId:              order.OrderId,
                lines:                lines,
                totalAmountCAD:       order.TotalAmountCAD,
                currencyCode:         order.CurrencyCode ?? "CAD",
                displayTotal:         order.DisplayTotal ?? order.TotalAmountCAD,
                status:               order.Status,
                estimatedDeliveryUtc: DateTime.UtcNow.AddDays(7));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order-confirmation email for order {OrderId}", orderId);
        }
    }

    private static string? BuildVariantInfo(ProductVariant? v)
    {
        if (v is null) return null;
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(v.Color)) parts.Add($"Color: {v.Color}");
        if (!string.IsNullOrEmpty(v.Size))  parts.Add($"Size: {v.Size}");
        return parts.Count == 0 ? null : string.Join(", ", parts);
    }

    private OrderDto MapToDto(Order order)
    {
        var items = order.Items.Select(i => new OrderItemDto(
            i.OrderItemId,
            i.ProductId,
            i.VariantId,
            i.Product?.Name ?? i.ProductName,
            i.Product?.Images.FirstOrDefault()?.ImageUrl,
            BuildVariantInfo(i.Variant),
            i.Quantity,
            i.UnitPriceCAD)).ToList();

        return new OrderDto(
            order.OrderId,
            order.Status,
            order.TotalAmountCAD,
            order.ShippingAmount,
            order.DiscountAmount,
            order.CouponCode,
            order.DisplayTotal ?? order.TotalAmountCAD,
            order.CurrencyCode ?? "CAD",
            order.CreatedAt,
            order.CancellationReason,
            items,
            order.PaymentMethod,
            order.PaymentTransactionId);
    }
}
