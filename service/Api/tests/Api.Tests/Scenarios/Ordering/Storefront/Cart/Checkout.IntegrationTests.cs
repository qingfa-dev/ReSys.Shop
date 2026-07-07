using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Ordering.Storefront.Cart;

public sealed class CheckoutIntegrationTests(ApiFixture fixture) : OrderingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Checkout_WithoutAuth_Returns401()
    {
        var request = new { paymentIntentId = (string?)null };
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/storefront/cart/checkout", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
