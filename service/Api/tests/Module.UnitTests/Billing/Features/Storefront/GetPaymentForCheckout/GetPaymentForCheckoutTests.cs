using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Billing.Services.Provider;

using PaymentCapture = Module.Billing.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Features.Storefront.GetPaymentForCheckout;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "GetPaymentForCheckout")]
public class GetPaymentForCheckoutTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPaymentForCheckoutQueryHandler _handler;

    public GetPaymentForCheckoutTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPaymentForCheckoutQueryHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Lookup by payment Id returns offline pending COD")]
    public async Task Handle_ShouldReturnOfflinePending_WhenLookupById()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(50m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Pending;
        payment.ProviderKey = GatewayConstants.Providers.CashOnDelivery;
        payment.ResponseCode = "COD-REF-001";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPaymentForCheckoutQuery { PaymentIntentId = payment.Id.ToString(), OrderId = orderId },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPending.Should().BeTrue();
        result.Value.IsOffline.Should().BeTrue();
        result.Value.IsCompleted.Should().BeFalse();
        result.Value.Amount.Should().Be(50m);
    }

    [Fact(DisplayName = "Handler: Lookup by ResponseCode matches non-Guid reference")]
    public async Task Handle_ShouldMatchByResponseCode()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(50m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Pending;
        payment.ProviderKey = GatewayConstants.Providers.CashOnDelivery;
        payment.ResponseCode = "COD-REF-001";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPaymentForCheckoutQuery { PaymentIntentId = "COD-REF-001", OrderId = orderId },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPending.Should().BeTrue();
        result.Value.IsOffline.Should().BeTrue();
        result.Value.Amount.Should().Be(50m);
    }

    [Fact(DisplayName = "Handler: Stripe gateway payment is not offline")]
    public async Task Handle_ShouldReturnNotOffline_WhenStripeGateway()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(80m, Guid.NewGuid(), orderId).Value;
        payment.State = PaymentRecordState.Processing;
        payment.ProviderKey = GatewayConstants.Providers.Stripe;
        payment.ResponseCode = "pi_checkout_gateway";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetPaymentForCheckoutQuery { PaymentIntentId = "pi_checkout_gateway", OrderId = orderId },
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsOffline.Should().BeFalse();
        result.Value.IsPending.Should().BeFalse();
        result.Value.IsCompleted.Should().BeFalse();
    }
}
