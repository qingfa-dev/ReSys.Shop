using FluentAssertions;
using Module.Billing.Services.Provider;
using Module.Billing.Services.Processing;

namespace Module.UnitTests.Payment.Services.Models;

public class PaymentGatewayResponseTests
{
    [Fact]
    public void Constructor_Should_Set_ClientSecret_When_Provided()
    {
        var response = new PaymentGatewayResponse(
            provider: "bogus",
            authorization: "auth_123",
            clientSecret: "pi_fake_secret_123");

        response.ClientSecret.Should().Be("pi_fake_secret_123");
    }
}
