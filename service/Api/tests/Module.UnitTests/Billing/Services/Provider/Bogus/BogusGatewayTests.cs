using Microsoft.Extensions.Options;

using Module.Billing.Services.Provider;
using Module.Billing.Services.Provider.Bogus;

namespace Module.UnitTests.Payment.Services.Provider.Bogus;

public class BogusGatewayTests
{
    [Fact]
    public async Task PurchaseAsync_With_Success_Card_Returns_ClientSecret()
    {
        var gateway = new BogusGateway(Options.Create(new BogusSetting()));

        var result = await gateway.PurchaseAsync(
            10.00m,
            BogusGateway.TestCards.Success,
            new GatewayOptions
            {
                Email = "test@example.com",
                Customer = "test@example.com",
                OrderId = "ORD-TEST",
                PaymentId = "PAY-TEST",
                IdempotencyKey = "test-key"
            });

        result.IsSuccess.Should().BeTrue();
        result.Value.ClientSecret.Should().NotBeNullOrEmpty();
        result.Value.ClientSecret.Should().StartWith("pi_fake_");
    }
}
