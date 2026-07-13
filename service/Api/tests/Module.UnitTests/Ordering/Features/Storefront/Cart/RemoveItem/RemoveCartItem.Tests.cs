using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.RemoveItem;

namespace Module.UnitTests.Ordering.Features.Storefront.Cart.RemoveItem;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "RemoveCartItem")]
public class RemoveCartItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly RemoveCartItem.CommandHandler _handler;
    private readonly Guid _userId;

    public RemoveCartItemTests()
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
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("customer");
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());

        _handler = new RemoveCartItem.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should remove item from cart")]
    public async Task Handle_ShouldRemoveItem_WhenItemExists()
    {
        // Arrange: Seed cart with line item
        var cart = OrderMethod.Create("USD", _userId, Guid.Empty).Value;
        var lineItem = new Module.Ordering.Domain.LineItems.LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = cart.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 2,
            Price = 19.99m,
            Total = 39.98m,
            Currency = "USD"
        };
        cart.LineItems.Add(lineItem);
        _dbContext.Set<Order>().Add(cart);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new RemoveCartItem.Command(lineItem.Id),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var cartAfter = await _dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .FirstAsync(o => o.Id == cart.Id, TestContext.Current.CancellationToken);
        cartAfter.LineItems.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return failure when item not found")]
    public async Task Handle_ShouldFail_WhenItemNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new RemoveCartItem.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(f => f.Code == "Order.NotFound");
    }

    [Fact(DisplayName = "Handler: Should return failure when user not authenticated")]
    public async Task Handle_ShouldFail_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        // Act
        var result = await _handler.Handle(
            new RemoveCartItem.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
