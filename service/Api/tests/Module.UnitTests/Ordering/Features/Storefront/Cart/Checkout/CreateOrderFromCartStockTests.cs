using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations;

using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Checkout;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.Checkout;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CreateOrderFromCart")]
public class CreateOrderFromCartStockTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CreateOrderFromCart.CommandHandler>> _loggerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CreateOrderFromCart.CommandHandler _handler;

    public CreateOrderFromCartStockTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly,
            typeof(StockItem).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("customer");
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _loggerMock = new Mock<ILogger<CreateOrderFromCart.CommandHandler>>();
        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new CreateOrderFromCart.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object, _notificationServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Stock: Should return insufficient stock when quantity exceeds stock", Skip = "Requires PostgreSQL — ExecuteUpdateAsync not supported by InMemory provider")]
    public async Task Handle_ShouldReturnInsufficientStock_WhenQuantityExceedsStock()
    {
        // Arrange: Seed location and limited stock
        var location = StockLocationMethod.Create("Warehouse").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var variantId = Guid.NewGuid();
        var stockItem = StockItemMethod.Create(stockLocationId: location.Id, variantId: variantId, countOnHand: 1).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Arrange: Create a draft cart requesting more than the available stock
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderExtensions.Create("USD", userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.Confirm;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "test@test.com";
        cart.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = variantId,
            Quantity = 5,
            Price = 29.99m,
            Total = 149.95m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(StockItemResult.Errors.InsufficientStock.Code);
    }

    [Fact(DisplayName = "Stock: Should return insufficient stock when concurrent checkouts exceed single item", Skip = "Requires PostgreSQL — ExecuteUpdateAsync not supported by InMemory provider")]
    public async Task Handle_Concurrent_Checkouts_Should_Not_Oversell()
    {
        // Arrange: Seed location and single unit of stock
        var location = StockLocationMethod.Create("Warehouse").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var variantId = Guid.NewGuid();
        var stockItem = StockItemMethod.Create(stockLocationId: location.Id, variantId: variantId, countOnHand: 1).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Arrange: Create two draft carts, each requesting the single unit
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);

        var cart1 = OrderExtensions.Create("USD", userId, Guid.Empty).Value;
        cart1.CheckoutState = CheckoutState.Confirm;
        cart1.BillAddressId = Guid.NewGuid();
        cart1.ShipAddressId = Guid.NewGuid();
        cart1.ShippingMethodId = Guid.NewGuid();
        cart1.Email = "test@test.com";
        cart1.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart1.Id,
            VariantId = variantId,
            Quantity = 1,
            Price = 29.99m,
            Total = 29.99m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart1);

        var cart2 = OrderExtensions.Create("USD", userId, Guid.Empty).Value;
        cart2.CheckoutState = CheckoutState.Confirm;
        cart2.BillAddressId = Guid.NewGuid();
        cart2.ShipAddressId = Guid.NewGuid();
        cart2.ShippingMethodId = Guid.NewGuid();
        cart2.Email = "test@test.com";
        cart2.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart2.Id,
            VariantId = variantId,
            Quantity = 1,
            Price = 29.99m,
            Total = 29.99m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart2);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act: Send both checkouts concurrently
        var task1 = _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request { PaymentIntentId = null }), TestContext.Current.CancellationToken);
        var task2 = _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request { PaymentIntentId = null }), TestContext.Current.CancellationToken);

        var results = await Task.WhenAll(task1, task2);
        var successes = results.Count(r => r.IsSuccess);

        // Assert: At most one checkout should succeed
        successes.Should().BeLessThanOrEqualTo(1);
    }
}
