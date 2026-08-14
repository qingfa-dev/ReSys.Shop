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
            PaymentState = OrderPaymentState.Completed,
            AtUtc = atUtc
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.PaymentCompletedAt.Should().Be(atUtc);
        updated.PaymentFailedAt.Should().BeNull();
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
            PaymentState = OrderPaymentState.Failed,
            AtUtc = DateTimeOffset.UtcNow
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.PaymentFailedAt.Should().NotBeNull();
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
            PaymentState = OrderPaymentState.Processing,
            AtUtc = DateTimeOffset.UtcNow
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.PaymentProcessingAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Unknown order returns NotFound")]
    public async Task UnknownOrder_ReturnsNotFound()
    {
        var result = await _handler.Handle(new RecordOrderPaymentStateCommand
        {
            OrderId = Guid.NewGuid(),
            PaymentState = OrderPaymentState.Completed,
            AtUtc = DateTimeOffset.UtcNow
        }, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
