using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Complete;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Complete;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "CompleteOrder")]
public class CompleteOrderTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly CompleteOrder.CommandHandler _handler;

    public CompleteOrderTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _handler = new CompleteOrder.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should complete a placed order and return populated detail collections")]
    public async Task Handle_ShouldCompleteOrder_WhenPlaced()
    {
        var order = await SeedOrderWithCollections(OrderStatus.Placed);

        var result = await _handler.Handle(
            new CompleteOrder.Command(order.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(OrderStatus.Placed);
        result.Value.CompletedAtUtc.Should().NotBeNull();
        result.Value.LineItems.Should().ContainSingle();
        result.Value.Adjustments.Should().ContainSingle();
        result.Value.Payments.Should().ContainSingle();
        result.Value.Shipments.Should().ContainSingle();
    }

    [Fact(DisplayName = "Handler: Should return not found when order missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new CompleteOrder.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OrderResult.Errors.NotFound(Guid.NewGuid()).Code);
    }

    private async Task<Order> SeedOrderWithCollections(OrderStatus status)
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", userId: Guid.NewGuid()).Value;
        order.Status = status;
        _dbContext.Set<Order>().Add(order);

        var lineItem = new LineItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            VariantId = Guid.NewGuid(),
            Quantity = 1,
            Price = 10m,
            Total = 10m,
            Currency = "USD",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
        _dbContext.Set<LineItem>().Add(lineItem);

        var adjustment = AdjustmentMethod.Create("Discount", 2m, order.Id, "Order", Guid.NewGuid(), "Manual", order.Id).Value;
        _dbContext.Set<Adjustment>().Add(adjustment);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        var shipment = ShipmentMethod.Create(order.Id, method.Id).Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(shipment);

        var payment = PaymentCaptureMethod.Create(10m, Guid.NewGuid(), order.Id).Value;
        _dbContext.Set<PaymentCapture>().Add(payment);

        await _dbContext.SaveChangesAsync(ct);
        return order;
    }
}
