using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.RecordOrderPaymentState;

namespace Module.UnitTests.Ordering.Features.Storefront.RecordOrderPaymentState;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "RecordOrderPaymentState")]
public class RecordOrderPaymentStateTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RecordOrderPaymentStateCommandHandler _handler;

    public RecordOrderPaymentStateTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new RecordOrderPaymentStateCommandHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Completed mirrors PaymentCompletedAt onto the order")]
    public async Task Completed_StampsPaymentCompletedAt()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var atUtc = new DateTimeOffset(2026, 8, 14, 10, 30, 0, TimeSpan.Zero);
        var result = await _handler.Handle(new RecordOrderPaymentStateCommand
        {
            OrderId = order.Id,
            PaymentState = PaymentTimelineState.Completed,
            AtUtc = atUtc
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.PaymentCompletedAtUtc.Should().Be(atUtc);
        updated.PaymentFailedAtUtc.Should().BeNull();
    }

    [Fact(DisplayName = "Failed mirrors PaymentFailedAt onto the order")]
    public async Task Failed_StampsPaymentFailedAt()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RecordOrderPaymentStateCommand
        {
            OrderId = order.Id,
            PaymentState = PaymentTimelineState.Failed,
            AtUtc = DateTimeOffset.UtcNow
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.PaymentFailedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Processing mirrors PaymentProcessingAt onto the order")]
    public async Task Processing_StampsPaymentProcessingAt()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RecordOrderPaymentStateCommand
        {
            OrderId = order.Id,
            PaymentState = PaymentTimelineState.Processing,
            AtUtc = DateTimeOffset.UtcNow
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.PaymentProcessingAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Completed with a completed capture recomputes PaymentState to Paid")]
    public async Task Completed_WithCompletedCapture_RecomputesPaymentState()
    {
        const decimal amount = 100m;
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.ItemTotal = amount;
        order.Total = amount;
        _dbContext.Set<Order>().Add(order);

        var capture = PaymentCaptureMethod.Create(amount, Guid.NewGuid(), order.Id).Value;
        capture.State = PaymentRecordState.Completed;
        capture.CapturedAmount = amount;
        _dbContext.Set<PaymentCapture>().Add(capture);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RecordOrderPaymentStateCommand
        {
            OrderId = order.Id,
            PaymentState = PaymentTimelineState.Completed,
            AtUtc = DateTimeOffset.UtcNow
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.PaymentState.Should().Be(OrderPaymentState.Paid);
        updated.PaymentTotal.Should().Be(amount);
    }

    [Fact(DisplayName = "Unknown order returns NotFound")]
    public async Task UnknownOrder_ReturnsNotFound()
    {
        var result = await _handler.Handle(new RecordOrderPaymentStateCommand
        {
            OrderId = Guid.NewGuid(),
            PaymentState = PaymentTimelineState.Completed,
            AtUtc = DateTimeOffset.UtcNow
        }, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
