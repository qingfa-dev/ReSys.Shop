using IGatewayRegistry = Module.Billing.Services.Provider.IGatewayRegistry;

using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Admin.PaymentMethods.Create;

namespace Module.UnitTests.Payment.Features.Admin.PaymentMethods.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentMethodCreate")]
public class CreatePaymentMethodTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IGatewayRegistry> _gatewayRegistryMock;
    private readonly CreatePaymentMethod.CommandHandler _handler;

    public CreatePaymentMethodTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _gatewayRegistryMock = new Mock<IGatewayRegistry>();
        _gatewayRegistryMock.Setup(x => x.IsRegistered(It.IsAny<string>())).Returns(true);

        _handler = new CreatePaymentMethod.CommandHandler(_dbContext, _gatewayRegistryMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should create payment method")]
    public async Task Handle_ShouldCreate()
    {
        var request = new CreatePaymentMethod.Request
        {
            Name = "Stripe",
            ProviderKey = "CreditCard",
            AutoCapture = true
        };
        var result = await _handler.Handle(new CreatePaymentMethod.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Stripe");
        result.Value.Active.Should().BeTrue();
    }
}
