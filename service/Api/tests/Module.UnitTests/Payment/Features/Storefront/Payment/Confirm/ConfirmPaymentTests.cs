using Module.Payment.Domain.Payments;
using Module.Payment.Features.Storefront.Payment.Confirm;
using PaymentRecord = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Confirm;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "ConfirmPayment")]
public class ConfirmPaymentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ConfirmPayment.CommandHandler _handler;

    public ConfirmPaymentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentRecord).Assembly];
        _dbContext = new ApplicationDbContext(options);
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        currentUserMock.Setup(x => x.UserName).Returns("test-user");
        _handler = new ConfirmPayment.CommandHandler(_dbContext, currentUserMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should confirm payment when in Pending state")]
    public async Task Handle_ShouldConfirm_WhenPending()
    {
        var payment = PaymentFactory.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Pend();
        _dbContext.Set<PaymentRecord>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ConfirmPayment.Command(payment.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(PaymentRecordState.Completed);
    }

    [Fact(DisplayName = "Handler: Should return failure when payment in Checkout state")]
    public async Task Handle_ShouldFail_WhenCheckout()
    {
        var payment = PaymentFactory.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<PaymentRecord>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ConfirmPayment.Command(payment.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when payment already completed")]
    public async Task Handle_ShouldFail_WhenAlreadyCompleted()
    {
        var payment = PaymentFactory.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        payment.Process();
        payment.Complete();
        _dbContext.Set<PaymentRecord>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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
}
