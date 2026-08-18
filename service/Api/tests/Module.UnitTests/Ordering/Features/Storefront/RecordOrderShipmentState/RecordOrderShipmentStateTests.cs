using Shared.Application.Domain.Orders;

using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.RecordOrderShipmentState;

namespace Module.UnitTests.Ordering.Features.Storefront.RecordOrderShipmentState;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "RecordOrderShipmentState")]
public class RecordOrderShipmentStateTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RecordOrderShipmentStateCommandHandler _handler;

    public RecordOrderShipmentStateTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new RecordOrderShipmentStateCommandHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: writes derived fulfillment state and mirrors shipped/delivered")]
    public async Task Handle_WritesFulfillmentStateAndMirrors()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var shippedAt = new DateTimeOffset(2026, 8, 14, 10, 0, 0, TimeSpan.Zero);
        var result = await _handler.Handle(new RecordOrderShipmentStateCommand
        {
            OrderId = order.Id,
            FulfillmentState = ShipmentState.Shipped,
            ShippedAtUtc = shippedAt
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.ShipmentState.Should().Be(ShipmentState.Shipped);
        updated.ShipmentShippedAtUtc.Should().Be(shippedAt);
    }

    [Fact(DisplayName = "Handler: mirrors are first-write only")]
    public async Task Handle_MirrorFirstWriteWins()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var first = new DateTimeOffset(2026, 8, 14, 10, 0, 0, TimeSpan.Zero);
        var later = first.AddHours(1);
        await _handler.Handle(new RecordOrderShipmentStateCommand
        {
            OrderId = order.Id,
            FulfillmentState = ShipmentState.Shipped,
            ShippedAtUtc = first
        }, TestContext.Current.CancellationToken);
        await _handler.Handle(new RecordOrderShipmentStateCommand
        {
            OrderId = order.Id,
            FulfillmentState = ShipmentState.Delivered,
            ShippedAtUtc = later
        }, TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.ShipmentShippedAtUtc.Should().Be(first);
        updated.ShipmentState.Should().Be(ShipmentState.Delivered);
    }

    [Fact(DisplayName = "Handler: delivered fulfillment auto-completes a placed order")]
    public async Task Handle_Delivered_AutoCompletesPlacedOrder()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.Status = OrderStatus.Placed;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RecordOrderShipmentStateCommand
        {
            OrderId = order.Id,
            FulfillmentState = ShipmentState.Delivered
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.Status.Should().Be(OrderStatus.Completed);
        updated.ModifiedBy.Should().Be("System");
    }

    [Fact(DisplayName = "Handler: non-delivered fulfillment keeps a placed order in Placed state")]
    public async Task Handle_Shipped_KeepsPlacedOrderPlaced()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.Status = OrderStatus.Placed;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RecordOrderShipmentStateCommand
        {
            OrderId = order.Id,
            FulfillmentState = ShipmentState.Shipped
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.Status.Should().Be(OrderStatus.Placed);
    }

    [Fact(DisplayName = "Handler: unknown order returns NotFound")]
    public async Task Handle_UnknownOrder_ReturnsNotFound()
    {
        var result = await _handler.Handle(new RecordOrderShipmentStateCommand
        {
            OrderId = Guid.NewGuid(),
            FulfillmentState = ShipmentState.Pending
        }, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
