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
public class CreateOrderFromCartTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CreateOrderFromCart.CommandHandler>> _loggerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CreateOrderFromCart.CommandHandler _handler;

    public CreateOrderFromCartTests()
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

    [Fact(DisplayName = "Handler: Should create order from cart successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenCartHasItems()
    {
        // Arrange: Seed location and stock
        var location = StockLocationMethod.Create("Warehouse").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var variantId = Guid.NewGuid();
        var stockItem = StockItemMethod.Create(stockLocationId: location.Id, variantId: variantId, countOnHand: 10).Value;
        _dbContext.Set<StockItem>().Add(stockItem);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Arrange: Create a draft cart with a line item
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
            Quantity = 2,
            Price = 29.99m,
            Total = 59.98m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(cart.Id);
        result.Value.Number.Should().StartWith("R");

        // Verify order is now placed
        var persisted = await _dbContext.Set<Order>().FindAsync(new object[] { cart.Id }, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(OrderStatus.Placed);

        // Verify stock decremented
        var si = await _dbContext.Set<StockItem>().FindAsync(new object[] { stockItem.Id }, TestContext.Current.CancellationToken);
        si.Should().NotBeNull();
        si!.CountOnHand.Should().Be(8);
    }

    [Fact(DisplayName = "Handler: Should return failure when cart is empty")]
    public async Task Handle_ShouldReturnFailure_WhenCartEmpty()
    {
        // Arrange: Create empty draft cart (checkout prerequisites set but no items)
        var userId = Guid.Parse(_currentUserMock.Object.UserId!);
        var cart = OrderExtensions.Create("USD", userId, Guid.Empty).Value;
        cart.CheckoutState = CheckoutState.Confirm;
        cart.BillAddressId = Guid.NewGuid();
        cart.ShipAddressId = Guid.NewGuid();
        cart.ShippingMethodId = Guid.NewGuid();
        cart.Email = "test@test.com";
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.EmptyOrderCannotFinalize.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when no cart exists")]
    public async Task Handle_ShouldReturnFailure_WhenNoCart()
    {
        // Act
        var result = await _handler.Handle(new CreateOrderFromCart.Command(new CreateOrderFromCart.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.Empty).Code);
    }
}
