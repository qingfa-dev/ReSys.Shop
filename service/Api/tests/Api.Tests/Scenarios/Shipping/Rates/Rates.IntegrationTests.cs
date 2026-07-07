using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Shipping.Rates;

public sealed class ShippingRatesIntegrationTests(ApiFixture fixture) : ShippingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ListShippingRates_WithAuth_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            "/api/storefront/shipping/rates?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListShippingRates_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/shipping/rates");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
