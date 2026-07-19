using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateCartItemQuantity")]
public class UpdateCartItemQuantityTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<UpdateCartItemQuantity.CommandHandler>> _loggerMock;
    private readonly UpdateCartItemQuantity.CommandHandler _handler;
    private readonly Guid _userId;
    private readonly Guid _variantId;
    private readonly Guid _lineItemId;

    public UpdateCartItemQuantityTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly,
            typeof(StockItem).Assembly
        ];
        _dbContext = new ApplicationDbContext(options);

        _userId = Guid.NewGuid();
        _variantId = Guid.NewGuid();
        _lineItemId = Guid.NewGuid();

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("customer");
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _senderMock = new Mock<ISender>();
        _loggerMock = new Mock<ILogger<UpdateCartItemQuantity.CommandHandler>>();
        _handler = new UpdateCartItemQuantity.CommandHandler(
            _dbContext, _loggerMock.Object, _currentUserMock.Object, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update item quantity")]
    public async Task Handle_ShouldUpdateQuantity_WhenItemExists()
    {
        // Arrange: Seed cart with line item
        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var cart = OrderMethod.Create("USD", _userId, Guid.Empty).Value;
        cart.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = _lineItemId,
            OrderId = cart.Id,
            VariantId = _variantId,
            Quantity = 2,
            Price = 19.99m,
            Total = 39.98m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock
            .Setup(x => x.Send(
                It.Is<CheckStockAvailability.Query>(
                    q => q.Request.VariantId == _variantId && q.Request.Quantity == 5),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CheckStockAvailability.Response>.Ok(
                new CheckStockAvailability.Response { VariantId = _variantId, IsAvailable = true, TotalAvailable = 10 }));

        // Act
        var result = await _handler.Handle(
            new UpdateCartItemQuantity.Command(_lineItemId, new UpdateCartItemQuantity.Request { Quantity = 5 }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var lineItem = await _dbContext.Set<Module.Ordering.Domain.LineItems.LineItem>()
            .FirstAsync(li => li.Id == _lineItemId, TestContext.Current.CancellationToken);
        lineItem.Quantity.Should().Be(5);
        lineItem.Total.Should().Be(19.99m * 5);
    }

    [Fact(DisplayName = "Handler: Should fail when quantity exceeds stock")]
    public async Task Handle_ShouldFail_WhenInsufficientStock()
    {
        // Arrange: Seed cart with line item
        var location = StockLocationMethod.Create("Main").Value;
        _dbContext.Set<StockLocation>().Add(location);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var cart = OrderMethod.Create("USD", _userId, Guid.Empty).Value;
        cart.LineItems.Add(new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = _lineItemId,
            OrderId = cart.Id,
            VariantId = _variantId,
            Quantity = 1,
            Price = 19.99m,
            Total = 19.99m,
            Currency = "USD"
        });
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _senderMock
            .Setup(x => x.Send(
                It.Is<CheckStockAvailability.Query>(
                    q => q.Request.VariantId == _variantId && q.Request.Quantity == 10),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CheckStockAvailability.Response>.Ok(
                new CheckStockAvailability.Response { VariantId = _variantId, IsAvailable = false, TotalAvailable = 3 }));

        // Act
        var result = await _handler.Handle(
            new UpdateCartItemQuantity.Command(_lineItemId, new UpdateCartItemQuantity.Request { Quantity = 10 }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when line item not found in cart")]
    public async Task Handle_ShouldFail_WhenItemNotFound()
    {
        // Arrange: Create cart but no matching line item
        var cart = OrderMethod.Create("USD", _userId, Guid.Empty).Value;
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new UpdateCartItemQuantity.Command(Guid.NewGuid(), new UpdateCartItemQuantity.Request { Quantity = 1 }),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
