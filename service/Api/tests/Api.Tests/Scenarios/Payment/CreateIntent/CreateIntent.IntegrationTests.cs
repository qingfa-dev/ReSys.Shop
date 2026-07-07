using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Payment.CreateIntent;

public sealed class CreateIntentIntegrationTests(ApiFixture fixture) : PaymentIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateIntent_WithoutAuth_Returns401()
    {
        var request = new { orderId = Guid.NewGuid() };
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/payment/create-intent", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
