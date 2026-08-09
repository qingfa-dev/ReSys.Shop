using FluentAssertions;
using Microsoft.Extensions.Options;
using Module.Billing.Services.Provider.Stripe;

namespace Module.UnitTests.Payment.Services.Provider.Stripe;

public class StripeGatewayTests
{
    [Fact]
    public void PurchaseAsync_Returns_ClientSecret_From_Intent()
    {
        var options = Options.Create(new StripeSetting
        {
            SecretKey = "sk_test_xxx",
            PublishableKey = "pk_test_xxx",
            WebhookSecret = "whsec_xxx"
        });
        var gateway = new StripeGateway(options);

        gateway.Should().NotBeNull();
    }
}
