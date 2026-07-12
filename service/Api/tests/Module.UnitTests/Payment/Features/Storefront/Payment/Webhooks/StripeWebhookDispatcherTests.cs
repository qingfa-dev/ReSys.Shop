using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Payment.Features.Storefront.Payment.Webhooks;
using Module.Payment.Services.Provider.Stripe;
using Module.Payment.Services.Webhook;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Webhooks;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
public class StripeWebhookDispatcherTests
{
    [Fact(DisplayName = "Dispatcher: provider name is stripe")]
    public void Provider_ReturnsStripe()
    {
        var dispatcher = new StripeWebhookDispatcher(
            Options.Create(new StripeSetting { WebhookSecret = "whsec_test" }),
            new Mock<ISender>().Object,
            new Mock<ILogger<StripeWebhookDispatcher>>().Object);

        dispatcher.Provider.Should().Be("stripe");
    }

    [Fact(DisplayName = "Dispatcher: returns NotConfigured when secret is empty")]
    public async Task HandleAsync_EmptySecret_ReturnsNotConfigured()
    {
        var dispatcher = new StripeWebhookDispatcher(
            Options.Create(new StripeSetting { WebhookSecret = "" }),
            new Mock<ISender>().Object,
            new Mock<ILogger<StripeWebhookDispatcher>>().Object);

        var result = await dispatcher.HandleAsync("payment_intent.succeeded", "{}", TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Stripe.WebhookSecret.NotConfigured");
    }

    [Fact(DisplayName = "Dispatcher: dispatches to real handler via ISender")]
    public async Task HandleAsync_DispatchesStripeWebhookCommand()
    {
        var sender = new Mock<ISender>();
        sender.Setup(x => x.Send(It.IsAny<StripeWebhook.Command>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(Result.Ok());

        var dispatcher = new StripeWebhookDispatcher(
            Options.Create(new StripeSetting { WebhookSecret = "whsec_test" }),
            sender.Object,
            new Mock<ILogger<StripeWebhookDispatcher>>().Object);

        var result = await dispatcher.HandleAsync("payment_intent.succeeded", "{}", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        sender.Verify(x => x.Send(
            It.Is<StripeWebhook.Command>(c => c.Payload == "{}" && c.StripeSignature == "stripe-signature"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
