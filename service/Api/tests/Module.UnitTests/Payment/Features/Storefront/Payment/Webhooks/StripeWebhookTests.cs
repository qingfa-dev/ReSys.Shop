using Module.Payment.Features.Storefront.Payment.Webhooks;
using Module.Payment.Infrastructure.Gateways.Stripe;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Webhooks;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "StripeWebhook")]
public class StripeWebhookTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStripeWebhookService> _webhookMock;
    private readonly StripeWebhook.CommandHandler _handler;

    public StripeWebhookTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(opts);
        _webhookMock = new Mock<IStripeWebhookService>();
        _webhookMock.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _handler = new StripeWebhook.CommandHandler(_dbContext, _webhookMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Webhook: invalid signature returns failure")]
    public async Task Handle_ShouldFail_WhenInvalidSignature()
    {
        _webhookMock.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var result = await _handler.Handle(new StripeWebhook.Command("{}", "invalid"), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Stripe.Webhook.InvalidSignature");
    }

    [Fact(DisplayName = "Webhook: unknown event returns success")]
    public async Task Handle_ShouldSucceed_ForUnknownEvent()
    {
        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>())).Returns(new global::Stripe.Event { Type = "unknown" });
        var result = await _handler.Handle(new StripeWebhook.Command("{}", "valid"), TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Webhook: unparseable payload returns failure")]
    public async Task Handle_ShouldFail_WhenUnparseable()
    {
        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>())).Returns((global::Stripe.Event?)null);
        var result = await _handler.Handle(new StripeWebhook.Command("bad", "valid"), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Stripe.Webhook.InvalidPayload");
    }
}
