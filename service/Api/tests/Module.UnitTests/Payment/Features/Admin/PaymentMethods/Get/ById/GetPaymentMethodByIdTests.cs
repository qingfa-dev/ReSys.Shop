using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Admin.PaymentMethods.Get.ById;

namespace Module.UnitTests.Payment.Features.Admin.PaymentMethods.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentMethodGetById")]
public class GetPaymentMethodByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPaymentMethodById.QueryHandler _handler;

    public GetPaymentMethodByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPaymentMethodById.QueryHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should return payment method by ID")]
    public async Task Handle_ShouldReturn_WhenExists()
    {
        var method = PaymentMethodMethod.Create("Stripe", null, "CreditCard").Value;
        _dbContext.Set<PaymentMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetPaymentMethodById.Query(method.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Stripe");
    }

    [Fact(DisplayName = "Handler: Should return NotFound when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var result = await _handler.Handle(new GetPaymentMethodById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }
}
