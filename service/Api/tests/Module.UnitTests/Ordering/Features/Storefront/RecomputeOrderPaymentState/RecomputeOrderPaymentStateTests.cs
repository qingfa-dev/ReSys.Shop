using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.RecomputeOrderPaymentState;

namespace Module.UnitTests.Ordering.Features.Storefront.RecomputeOrderPaymentState;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "RecomputeOrderPaymentState")]
public class RecomputeOrderPaymentStateTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RecomputeOrderPaymentStateCommandHandler _handler;

    public RecomputeOrderPaymentStateTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new RecomputeOrderPaymentStateCommandHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handle: recomputes payment state from captures after a refund")]
    public async Task Handle_RecomputesPaymentState_AfterRefund()
    {
        const decimal amount = 100m;
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.ItemTotal = amount;
        order.Total = amount;
        _dbContext.Set<Order>().Add(order);

        var capture = PaymentCaptureMethod.Create(amount, Guid.NewGuid(), order.Id).Value;
        capture.State = PaymentRecordState.Completed;
        capture.CapturedAmount = amount;
        capture.RefundedAmount = 30m;
        _dbContext.Set<PaymentCapture>().Add(capture);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RecomputeOrderPaymentStateCommand
        {
            OrderId = order.Id
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var updated = await _dbContext.Set<Order>().FirstAsync(o => o.Id == order.Id);
        updated.PaymentTotal.Should().Be(70m);
        updated.OutstandingBalance.Should().Be(30m);
        updated.PaymentState.Should().Be(OrderPaymentState.BalanceDue);
    }

    [Fact(DisplayName = "Unknown order returns NotFound")]
    public async Task UnknownOrder_ReturnsNotFound()
    {
        var result = await _handler.Handle(new RecomputeOrderPaymentStateCommand
        {
            OrderId = Guid.NewGuid()
        }, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
