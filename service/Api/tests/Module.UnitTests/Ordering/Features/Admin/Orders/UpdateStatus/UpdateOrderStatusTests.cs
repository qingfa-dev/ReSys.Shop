using Shared.Application.Contracts.Inventory;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.UpdateStatus;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.UpdateStatus;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "UpdateOrderStatus")]
public class UpdateOrderStatusTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<UpdateOrderStatus.CommandHandler>> _loggerMock;
    private readonly Mock<IStockQuantityService> _stockCheckerMock;
    private readonly UpdateOrderStatus.CommandHandler _handler;

    public UpdateOrderStatusTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _loggerMock = new Mock<ILogger<UpdateOrderStatus.CommandHandler>>();
        _stockCheckerMock = new Mock<IStockQuantityService>();

        _handler = new UpdateOrderStatus.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object, _stockCheckerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should place a draft order when transitioning to placed")]
    public async Task Handle_ShouldPlaceOrder_WhenDraftToPlaced()
    {
        // Arrange
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.LineItems.Add(new() { Id = Guid.NewGuid(), Quantity = 1, Price = 10, VariantId = Guid.NewGuid(), OrderId = order.Id });
        order.BillAddressId = Guid.NewGuid();
        order.ShipAddressId = Guid.NewGuid();
        order.ShippingMethodId = Guid.NewGuid();
        order.Email = "test@test.com";
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateOrderStatus.Request { Status = OrderStatus.Placed };

        // Act
        var result = await _handler.Handle(
            new UpdateOrderStatus.Command(order.Id, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Order>().FindAsync(new object[] { order.Id }, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(OrderStatus.Placed);
    }

    [Fact(DisplayName = "Handler: Should cancel an order")]
    public async Task Handle_ShouldCancelOrder_WhenPlaced()
    {
        // Arrange: Create a placed order
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid(), storeId: Guid.Empty).Value;
        order.Status = OrderStatus.Placed;
        order.CompletedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateOrderStatus.Request { Status = OrderStatus.Canceled };

        // Act
        var result = await _handler.Handle(
            new UpdateOrderStatus.Command(order.Id, request),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Order>().FindAsync(new object[] { order.Id }, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact(DisplayName = "Handler: Should return failure when order not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var request = new UpdateOrderStatus.Request { Status = OrderStatus.Canceled };

        var result = await _handler.Handle(
            new UpdateOrderStatus.Command(Guid.NewGuid(), request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }
}
