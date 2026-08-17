using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Storefront.Payment.SetupIntent;
using Module.Billing.Services.Provider;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.SetupIntent;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "CreateSetupIntent")]
public class CreateSetupIntentTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly CreateSetupIntent.CommandHandler _handler;

    public CreateSetupIntentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.CreateSetupIntentAsync(
                It.IsAny<string?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResponse("stripe", setupIntentClientSecret: "seti_test_1"));

        var registryMock = new Mock<IGatewayRegistry>();
        registryMock.Setup(x => x.GetGateway(It.IsAny<string>()))
            .Returns(Result<IPaymentGatewayActionProvider>.Ok(_gatewayMock.Object));

        _handler = new CreateSetupIntent.CommandHandler(_dbContext, registryMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Returns state default (Checkout) for gateway response mapping")]
    public async Task Handle_WhenGatewaySucceeds_StateDefaultsToCheckout()
    {
        var pm = new PaymentMethod
        {
            Name = "Credit Card",
            ProviderKey = GatewayConstants.Providers.Stripe,
            Active = true,
            IsDeleted = false
        };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CreateSetupIntent.Command(pm.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(PaymentRecordState.Checkout);
        result.Value.ClientSecret.Should().Be("seti_test_1");
        result.Value.PaymentStatus.Should().BeNull();
    }

    [Fact(DisplayName = "Handler: Returns NotFound when payment method is inactive")]
    public async Task Handle_WhenPaymentMethodInactive_ReturnsNotFound()
    {
        var pm = new PaymentMethod
        {
            Name = "Inactive Card",
            ProviderKey = GatewayConstants.Providers.Stripe,
            Active = false
        };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new CreateSetupIntent.Command(pm.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}