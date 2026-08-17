using Module.Ordering.Domain.Orders;

using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Storefront.Payment.Status;


namespace Module.UnitTests.Payment.Features.Storefront.Payment.Status;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "GetPaymentStatus")]
public class GetPaymentStatusTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPaymentStatus.QueryHandler _handler;
    private readonly string _currentUserId;

    public GetPaymentStatusTests()
    {
        _currentUserId = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly, typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(x => x.UserId).Returns(_currentUserId);
        currentUserMock.Setup(x => x.UserName).Returns("test-user");

        _handler = new GetPaymentStatus.QueryHandler(_dbContext, currentUserMock.Object);
    }

    private async Task SeedOrderAsync(Guid orderId, CancellationToken ct)
    {
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.Parse(_currentUserId),
            Status = OrderStatus.Draft,
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    private static PaymentCapture CreateTestPayment(Guid orderId)
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.ResponseCode = "pi_test_123";
        return payment;
    }

    [Fact(DisplayName = "Handler: Should return completed status for completed payment")]
    public async Task Handle_ShouldReturnCompleted_WhenPaymentCompleted()
    {
        var orderId = Guid.NewGuid();
        var payment = CreateTestPayment(orderId);
        payment.Process();
        payment.Complete();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SeedOrderAsync(orderId, TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPaymentStatus.Query(orderId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompleted.Should().BeTrue();
        result.Value.State.Should().Be(PaymentRecordState.Completed);
        result.Value.Amount.Should().Be(100m);
    }

    [Fact(DisplayName = "Handler: Should return pending status for processing payment")]
    public async Task Handle_ShouldReturnPending_WhenProcessing()
    {
        var orderId = Guid.NewGuid();
        var payment = CreateTestPayment(orderId);
        payment.Process();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SeedOrderAsync(orderId, TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPaymentStatus.Query(orderId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompleted.Should().BeFalse();
        result.Value.State.Should().Be(PaymentRecordState.Processing);
    }

    [Fact(DisplayName = "Handler: Should return NotFound when order does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenOrderMissing()
    {
        var result = await _handler.Handle(
            new GetPaymentStatus.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return NotFound when order belongs to another user")]
    public async Task Handle_ShouldReturnNotFound_WhenOrderBelongsToAnotherUser()
    {
        var orderId = Guid.NewGuid();
        var otherOrder = new Order
        {
            Id = orderId,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Draft,
        };
        _dbContext.Set<Order>().Add(otherOrder);
        _dbContext.Set<PaymentCapture>().Add(CreateTestPayment(orderId));
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPaymentStatus.Query(orderId),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return NotFound when order has no payment capture")]
    public async Task Handle_ShouldReturnNotFound_WhenNoPayment()
    {
        var orderId = Guid.NewGuid();
        await SeedOrderAsync(orderId, TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPaymentStatus.Query(orderId),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}