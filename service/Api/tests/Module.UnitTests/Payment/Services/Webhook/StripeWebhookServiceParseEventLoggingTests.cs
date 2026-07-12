using Microsoft.Extensions.Options;
using Module.Payment.Services.Provider.Stripe;
using Module.Payment.Services.Webhook;

namespace Module.UnitTests.Payment.Services.Webhook;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
public class StripeWebhookServiceParseEventLoggingTests
{
    [Fact(DisplayName = "ParseEvent: logs error and returns null on malformed payload")]
    public void ParseEvent_MalformedPayload_LogsError_ReturnsNull()
    {
        var options = Options.Create(new StripeSetting { WebhookSecret = "whsec_test" });
        var logger = new Mock<ILogger<StripeWebhookHandler>>();
        var sut = new StripeWebhookHandler(options, logger.Object);

        var result = sut.ParseEvent("{not-valid-json");

        result.Should().BeNull();
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Stripe event parse failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
