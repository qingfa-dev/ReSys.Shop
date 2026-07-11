using Module.Ordering.Domain.Orders;
using IPaymentGatewayActionProvider = Module.Payment.Services.Provider.IPaymentGatewayActionProvider;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;

using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Storefront.Payment.Confirm;
using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Confirm;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "ConfirmPayment")]
public class ConfirmPaymentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ConfirmPayment.CommandHandler _handler;
    private readonly string _currentUserId;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly Mock<IGatewayRegistry> _gatewayRegistryMock;

    public ConfirmPaymentTests()
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
        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.GetPaymentStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("succeeded");

        _gatewayRegistryMock = new Mock<IGatewayRegistry>();
        _gatewayRegistryMock.Setup(x => x.GetGateway(It.IsAny<string>()))
            .Returns(Result<IPaymentGatewayActionProvider>.Ok(_gatewayMock.Object));

        _handler = new ConfirmPayment.CommandHandler(_dbContext, currentUserMock.Object, _gatewayRegistryMock.Object);
    }

    private async Task SeedOrderAsync(Guid orderId, CancellationToken ct)
    {
        var order = new Order
        {
            Id = orderId,
            UserId = Guid.Parse(_currentUserId),
            Status = OrderStatus.Placed,
        };
        _dbContext.Set<Order>().Add(order);
        await _dbContext.SaveChangesAsync(ct);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    private static PaymentCapture CreateTestPayment(Guid? paymentMethodId = null, Guid? orderId = null)
    {
        var payment = PaymentCaptureMethod.Create(100m, paymentMethodId ?? Guid.NewGuid(), orderId ?? Guid.NewGuid()).Value;
        payment.ResponseCode = "pi_test_123";
        return payment;
    }

    [Fact(DisplayName = "Handler: Should confirm payment when in Pending state")]
    public async Task Handle_ShouldConfirm_WhenPending()
    {
        var payment = CreateTestPayment();
        payment.Process();
        payment.Pend();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SeedOrderAsync(payment.OrderId, TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ConfirmPayment.Command(payment.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "Handler: Should return failure when gateway status is not succeeded")]
    public async Task Handle_ShouldFail_WhenGatewayStatusNotSucceeded()
    {
        _gatewayMock.Setup(x => x.GetPaymentStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("requires_payment_method");

        var payment = CreateTestPayment();
        payment.Process();
        payment.Pend();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SeedOrderAsync(payment.OrderId, TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ConfirmPayment.Command(payment.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.Confirm.NotSucceeded");
    }

    [Fact(DisplayName = "Handler: Should return failure when payment in Checkout state")]
    public async Task Handle_ShouldFail_WhenCheckout()
    {
        var payment = CreateTestPayment();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SeedOrderAsync(payment.OrderId, TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ConfirmPayment.Command(payment.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when payment already completed")]
    public async Task Handle_ShouldFail_WhenAlreadyCompleted()
    {
        var payment = CreateTestPayment();
        payment.Process();
        payment.Complete();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await SeedOrderAsync(payment.OrderId, TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ConfirmPayment.Command(payment.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Payment.AlreadyCompleted");
    }

    [Fact(DisplayName = "Handler: Should return NotFound when payment does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(
            new ConfirmPayment.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should fail when payment belongs to another user")]
    public async Task Handle_Should_Fail_When_Payment_Belongs_To_Another_User()
    {
        var otherUserId = Guid.NewGuid();
        var otherOrder = new Order
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            Status = OrderStatus.Placed,
        };
        _dbContext.Set<Order>().Add(otherOrder);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var payment = CreateTestPayment(orderId: otherOrder.Id);
        payment.Process();
        payment.Pend();
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ConfirmPayment.Command(payment.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
