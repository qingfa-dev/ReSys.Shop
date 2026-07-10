using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Payment.Confirm;

public sealed class ConfirmPaymentIntegrationTests(ApiFixture fixture) : PaymentIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ConfirmPayment_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/storefront/payment/confirm/{Guid.NewGuid()}", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
