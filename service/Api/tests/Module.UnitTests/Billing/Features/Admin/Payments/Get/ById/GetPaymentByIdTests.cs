using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Admin.Payments.Get.ById;
using PaymentCapture = Module.Billing.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Features.Admin.Payments.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "GetPaymentById")]
public class GetPaymentByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPaymentById.QueryHandler _handler;

    public GetPaymentByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPaymentById.QueryHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should return payment detail by ID")]
    public async Task Handle_ShouldReturnPayment_WhenExists()
    {
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetPaymentById.Query(payment.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(payment.Id);
        result.Value.Number.Should().Be(payment.Number);
        result.Value.Amount.Should().Be(100m);
    }

    [Fact(DisplayName = "Handler: Should return NotFound when payment does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(new GetPaymentById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }
}
