using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Shipping.Calculate;

public sealed class CalculateShippingIntegrationTests(ApiFixture fixture) : ShippingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CalculateShipping_WithoutAuth_Returns401()
    {
        var request = new { orderId = Guid.NewGuid(), shippingMethodId = Guid.NewGuid() };
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/shipping/calculate", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
