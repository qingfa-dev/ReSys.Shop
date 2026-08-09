using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Billing.Features.Storefront.Payment.Webhooks;
using Module.Billing.Services.Provider.Stripe;
using Module.Billing.Services.Webhook;

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

}
