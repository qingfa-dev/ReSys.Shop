using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Billing.Features.Storefront.Payment.Webhooks;
using Module.Billing.Services.Provider.Stripe;
using Module.Billing.Services.Webhook;

using Stripe;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Webhooks;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
public class StripeWebhookDispatcherTests
{
    private const string WebhookSecret = "whsec_test";
    private const string Payload = "{\"id\":\"evt_test_1\",\"object\":\"event\"}";

    private static string BuildSignatureHeader(string payload, string secret)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string signature = EventUtility.ComputeSignature(secret, timestamp.ToString(), payload);
        return $"t={timestamp},v1={signature}";
    }

    private static StripeWebhookDispatcher CreateDispatcher(string webhookSecret, string environmentName)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(environmentName);

        return new StripeWebhookDispatcher(
            Options.Create(new StripeSetting { WebhookSecret = webhookSecret }),
            new Mock<ILogger<StripeWebhookDispatcher>>().Object,
            environment.Object);
    }

    [Fact(DisplayName = "Dispatcher: accepts any signature in Development when webhook secret is empty")]
    public void ValidateSignature_DevelopmentNoSecret_Accepts()
    {
        var dispatcher = CreateDispatcher(string.Empty, Environments.Development);

        var result = dispatcher.ValidateSignature(Payload, "invalid-signature");

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Dispatcher: rejects signatures outside Development when webhook secret is empty")]
    public void ValidateSignature_NonDevelopmentNoSecret_Rejects()
    {
        var dispatcher = CreateDispatcher(string.Empty, Environments.Production);

        var result = dispatcher.ValidateSignature(Payload, "invalid-signature");

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Dispatcher: verifies a valid signature against the configured secret")]
    public void ValidateSignature_WithSecret_Verifies()
    {
        var dispatcher = CreateDispatcher(WebhookSecret, Environments.Production);
        string header = BuildSignatureHeader(Payload, WebhookSecret);

        var result = dispatcher.ValidateSignature(Payload, header);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Dispatcher: rejects a tampered payload against the configured secret")]
    public void ValidateSignature_WithSecret_TamperedPayload_Rejects()
    {
        var dispatcher = CreateDispatcher(WebhookSecret, Environments.Production);
        string header = BuildSignatureHeader(Payload, WebhookSecret);
        string tamperedPayload = Payload.Replace("evt_test_1", "evt_test_2");

        var result = dispatcher.ValidateSignature(tamperedPayload, header);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Dispatcher: rejects a tampered signature header against the configured secret")]
    public void ValidateSignature_WithSecret_TamperedSignature_Rejects()
    {
        var dispatcher = CreateDispatcher(WebhookSecret, Environments.Production);
        string header = BuildSignatureHeader(Payload, WebhookSecret);
        string tamperedHeader = header.Replace("v1=", "v1=deadbeef");

        var result = dispatcher.ValidateSignature(Payload, tamperedHeader);

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Dispatcher: Development with a configured secret still verifies a valid signature")]
    public void ValidateSignature_DevelopmentWithSecret_StillVerifies()
    {
        var dispatcher = CreateDispatcher(WebhookSecret, Environments.Development);
        string header = BuildSignatureHeader(Payload, WebhookSecret);

        var result = dispatcher.ValidateSignature(Payload, header);

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Dispatcher: Development with a configured secret rejects a tampered signature")]
    public void ValidateSignature_DevelopmentWithSecret_Tampered_Rejects()
    {
        var dispatcher = CreateDispatcher(WebhookSecret, Environments.Development);
        string header = BuildSignatureHeader(Payload, WebhookSecret);
        string tamperedHeader = header.Replace("v1=", "v1=deadbeef");

        var result = dispatcher.ValidateSignature(Payload, tamperedHeader);

        result.Should().BeFalse();
    }
}
