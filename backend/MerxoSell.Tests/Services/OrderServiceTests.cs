using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MerxoSell.API.DTOs.Orders;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.Tests.Services;

/// <summary>
/// OrderService exercises a real AppDbContext (InMemory) for stock, orders,
/// and status-history writes, while repositories are mocked.
/// </summary>
public class OrderServiceTests : TestBase
{
    private readonly Mock<IOrderRepository>              _orderRepo    = new();
    private readonly Mock<ICartRepository>               _cartRepo     = new();
    private readonly Mock<IOrderStatusHistoryRepository> _historyRepo  = new();
    private readonly Mock<ICouponService>                _couponSvc    = new();
    private readonly Mock<IEmailService>                 _email        = new();

    private OrderService BuildSut()
    {
        // Build a minimal DI scope factory pointing at the test's InMemory Db
        var services = new ServiceCollection();
        services.AddSingleton(Db);
        services.AddScoped<IOrderRepository>(_ => _orderRepo.Object);
        services.AddScoped<IEmailService>(_ => _email.Object);
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        return new OrderService(
            _orderRepo.Object, _cartRepo.Object, _historyRepo.Object,
            _couponSvc.Object, _email.Object, Db, scopeFactory,
            NullLogger<OrderService>.Instance);
    }

    private (Product product, Cart cart) SeedProductAndCart(int stock = 10)
    {
        var category = new Category { CategoryId = 100, Name = "Test", Slug = "test", IsActive = true };
        var seller   = new Seller   { SellerId = 20, UserId = SellerUser.UserId, StoreName = "Seed Store", IsActive = true };

        Db.Categories.Add(category);

        // Use a unique SellerId to avoid duplicate-key conflicts
        if (!Db.Sellers.Any(s => s.SellerId == seller.SellerId))
            Db.Sellers.Add(seller);

        Db.SaveChanges();

        var product = new Product
        {
            ProductId  = 200 + stock,
            CategoryId = category.CategoryId,
            SellerId   = seller.SellerId,
            Name       = "Test Product",
            Slug       = "test-product",
            BasePrice  = 50m,
            Stock      = stock,
            Status     = "Approved",
            IsActive   = true
        };

        Db.Products.Add(product);

        var cart = new Cart { CartId = 300 + stock, UserId = BuyerUser.UserId };
        Db.Carts.Add(cart);
        Db.SaveChanges();

        return (product, cart);
    }

    // ── CreateOrder stock decrement ───────────────────────────────────────────

    [Fact]
    public async Task CreateOrder_DecrementsProductStock()
    {
        var (product, cart) = SeedProductAndCart(stock: 10);

        _cartRepo.Setup(r => r.GetCartByUserIdAsync(BuyerUser.UserId)).ReturnsAsync(cart);
        _cartRepo.Setup(r => r.ClearCartAsync(cart.CartId)).Returns(Task.CompletedTask);
        _couponSvc.Setup(c => c.ValidateAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int?>()))
                  .ReturnsAsync(new MerxoSell.API.DTOs.Coupons.CouponValidationResultDto(true, "ok", null, 100m, 0m, 100m));

        var createdOrder = new Order
        {
            OrderId = 1, UserId = BuyerUser.UserId, Status = "Pending",
            TotalAmountCAD = 50m, Items = new List<OrderItem>()
        };
        _orderRepo.Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                  .ReturnsAsync((Order o) => { o.OrderId = 1; return o; });
        _orderRepo.Setup(r => r.GetByIdAsync(1, null)).ReturnsAsync(createdOrder);
        _historyRepo.Setup(r => r.RecordAsync(It.IsAny<OrderStatusHistory>()))
                    .ReturnsAsync((OrderStatusHistory h) => h);

        var dto = new CreateOrderDto(
            AddressId: 1, CurrencyCode: "CAD", DisplayTotal: 50m,
            ShippingAmount: 0m, DiscountAmount: 0m, CouponCode: null,
            Items: new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(ProductId: product.ProductId, VariantId: null, Quantity: 3)
            });

        await BuildSut().CreateOrderAsync(BuyerUser.UserId, dto);

        var updatedProduct = await Db.Products.FindAsync(product.ProductId);
        updatedProduct!.Stock.Should().Be(7);
    }

    [Fact]
    public async Task CreateOrder_InsufficientStock_ThrowsInvalidOperation()
    {
        var (product, cart) = SeedProductAndCart(stock: 2);

        _cartRepo.Setup(r => r.GetCartByUserIdAsync(BuyerUser.UserId)).ReturnsAsync(cart);

        var dto = new CreateOrderDto(
            AddressId: 1, CurrencyCode: "CAD", DisplayTotal: 50m,
            ShippingAmount: 0m, DiscountAmount: 0m, CouponCode: null,
            Items: new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(ProductId: product.ProductId, VariantId: null, Quantity: 5)
            });

        await BuildSut().Invoking(s => s.CreateOrderAsync(BuyerUser.UserId, dto))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*Not enough stock*");
    }

    [Fact]
    public async Task CreateOrder_EmptyItemsAndEmptyCart_ThrowsInvalidOperation()
    {
        var cart = new Cart { CartId = 500, UserId = BuyerUser.UserId, Items = new List<CartItem>() };
        _cartRepo.Setup(r => r.GetCartByUserIdAsync(BuyerUser.UserId)).ReturnsAsync(cart);

        var dto = new CreateOrderDto(
            AddressId: 1, CurrencyCode: "CAD", DisplayTotal: 0m,
            ShippingAmount: 0m, DiscountAmount: 0m, CouponCode: null,
            Items: new List<CreateOrderItemDto>());

        await BuildSut().Invoking(s => s.CreateOrderAsync(BuyerUser.UserId, dto))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*Cart is empty*");
    }

    [Fact]
    public async Task CreateOrder_SnapshotsCurrencyCode()
    {
        var (product, cart) = SeedProductAndCart(stock: 10);

        _cartRepo.Setup(r => r.GetCartByUserIdAsync(BuyerUser.UserId)).ReturnsAsync(cart);
        _cartRepo.Setup(r => r.ClearCartAsync(cart.CartId)).Returns(Task.CompletedTask);
        _historyRepo.Setup(r => r.RecordAsync(It.IsAny<OrderStatusHistory>()))
                    .ReturnsAsync((OrderStatusHistory h) => h);

        Order? capturedOrder = null;
        _orderRepo.Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                  .Callback<Order>(o => { o.OrderId = 99; capturedOrder = o; })
                  .ReturnsAsync((Order o) => o);

        var returnedOrder = new Order
        {
            OrderId = 99, UserId = BuyerUser.UserId, Status = "Pending",
            CurrencyCode = "USD", TotalAmountCAD = 50m, Items = new List<OrderItem>()
        };
        _orderRepo.Setup(r => r.GetByIdAsync(99, null)).ReturnsAsync(returnedOrder);

        var dto = new CreateOrderDto(
            AddressId: 1, CurrencyCode: "USD", DisplayTotal: 38m,
            ShippingAmount: 0m, DiscountAmount: 0m, CouponCode: null,
            Items: new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(ProductId: product.ProductId, VariantId: null, Quantity: 1)
            });

        var result = await BuildSut().CreateOrderAsync(BuyerUser.UserId, dto);

        capturedOrder!.CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public async Task CreateOrder_CreatesInitialStatusHistoryRow()
    {
        var (product, cart) = SeedProductAndCart(stock: 5);

        _cartRepo.Setup(r => r.GetCartByUserIdAsync(BuyerUser.UserId)).ReturnsAsync(cart);
        _cartRepo.Setup(r => r.ClearCartAsync(cart.CartId)).Returns(Task.CompletedTask);
        _orderRepo.Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                  .ReturnsAsync((Order o) => { o.OrderId = 10; return o; });
        _orderRepo.Setup(r => r.GetByIdAsync(10, null))
                  .ReturnsAsync(new Order { OrderId = 10, Status = "Pending", TotalAmountCAD = 50m, Items = new List<OrderItem>() });

        OrderStatusHistory? recorded = null;
        _historyRepo.Setup(r => r.RecordAsync(It.IsAny<OrderStatusHistory>()))
                    .Callback<OrderStatusHistory>(h => recorded = h)
                    .ReturnsAsync((OrderStatusHistory h) => h);

        var dto = new CreateOrderDto(
            AddressId: 1, CurrencyCode: "CAD", DisplayTotal: 50m,
            ShippingAmount: 0m, DiscountAmount: 0m, CouponCode: null,
            Items: new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(ProductId: product.ProductId, VariantId: null, Quantity: 1)
            });

        await BuildSut().CreateOrderAsync(BuyerUser.UserId, dto);

        recorded.Should().NotBeNull();
        recorded!.ToStatus.Should().Be("Pending");
        recorded.Note.Should().Contain("placed");
    }

    // ── CancelOrder ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelOrder_PendingOrder_SetsCancelledStatus()
    {
        var order = new Order
        {
            OrderId = 50, UserId = BuyerUser.UserId, Status = "Pending",
            TotalAmountCAD = 100m
        };
        Db.Orders.Add(order);
        Db.SaveChanges();

        _historyRepo.Setup(r => r.RecordAsync(It.IsAny<OrderStatusHistory>()))
                    .ReturnsAsync((OrderStatusHistory h) => h);

        await BuildSut().CancelOrderAsync(order.OrderId, "Changed my mind");

        var updated = await Db.Orders.FindAsync(order.OrderId);
        updated!.Status.Should().Be("Cancelled");
        updated.CancellationReason.Should().Be("Changed my mind");
    }

    [Fact]
    public async Task CancelOrder_DeliveredOrder_ThrowsInvalidOperation()
    {
        var order = new Order
        {
            OrderId = 51, UserId = BuyerUser.UserId, Status = "Delivered",
            TotalAmountCAD = 100m
        };
        Db.Orders.Add(order);
        Db.SaveChanges();

        await BuildSut().Invoking(s => s.CancelOrderAsync(order.OrderId, "Too late"))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*Delivered*");
    }

    [Fact]
    public async Task CancelOrder_EmptyReason_ThrowsInvalidOperation()
    {
        var order = new Order
        {
            OrderId = 52, UserId = BuyerUser.UserId, Status = "Pending",
            TotalAmountCAD = 100m
        };
        Db.Orders.Add(order);
        Db.SaveChanges();

        await BuildSut().Invoking(s => s.CancelOrderAsync(order.OrderId, "  "))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*cancellation reason is required*");
    }

    // ── UpdateOrderStatus ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateOrderStatus_InvalidStatus_ThrowsInvalidOperation()
    {
        var order = new Order
        {
            OrderId = 60, UserId = BuyerUser.UserId, Status = "Pending",
            TotalAmountCAD = 50m
        };
        Db.Orders.Add(order);
        Db.SaveChanges();

        await BuildSut().Invoking(s => s.UpdateOrderStatusAsync(order.OrderId, "Exploded"))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*Status must be one of*");
    }

    [Fact]
    public async Task UpdateOrderStatus_DeliveredOrderCannotMoveBack_Throws()
    {
        var user  = new User { UserId = BuyerUser.UserId };
        var order = new Order
        {
            OrderId = 61, UserId = BuyerUser.UserId, Status = "Delivered",
            TotalAmountCAD = 50m, User = BuyerUser
        };
        Db.Orders.Add(order);
        Db.SaveChanges();

        await BuildSut().Invoking(s => s.UpdateOrderStatusAsync(order.OrderId, "Processing"))
                        .Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*already Delivered*");
    }
}
