using Hangfire;

using IStripeWebhookService = Module.Payment.Services.Webhook.IStripeWebhookService;

using Module.Payment.Features.Storefront.Payment.Webhooks;
using Stripe;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Webhooks;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "StripeWebhook")]
public class StripeWebhookTests
{
    private readonly Mock<IStripeWebhookService> _webhookMock;
    private readonly Mock<IBackgroundJobClient> _bgJobClientMock;
    private readonly StripeWebhook.CommandHandler _handler;

    public StripeWebhookTests()
    {
        _webhookMock = new Mock<IStripeWebhookService>();
        _webhookMock.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _bgJobClientMock = new Mock<IBackgroundJobClient>();
        _handler = new StripeWebhook.CommandHandler(_webhookMock.Object, _bgJobClientMock.Object);
    }

    [Fact(DisplayName = "Webhook: invalid signature returns failure")]
    public async Task Handle_ShouldFail_WhenInvalidSignature()
    {
        _webhookMock.Setup(x => x.ValidateSignature(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var result = await _handler.Handle(new StripeWebhook.Command("{}", "invalid"), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Stripe.Webhook.InvalidSignature");
    }

    [Fact(DisplayName = "Webhook: unparseable payload returns failure")]
    public async Task Handle_ShouldFail_WhenUnparseable()
    {
        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>())).Returns((global::Stripe.Event?)null);
        var result = await _handler.Handle(new StripeWebhook.Command("bad", "valid"), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Stripe.Webhook.InvalidPayload");
    }

    [Fact(DisplayName = "Webhook: Returns Ok immediately after queueing")]
    public async Task Handle_ShouldReturnOk_ForValidSignature()
    {
        _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
            .Returns(new Event { Type = "payment_intent.succeeded" });

        var result = await _handler.Handle(
            new StripeWebhook.Command("{}", "sig"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }
}
