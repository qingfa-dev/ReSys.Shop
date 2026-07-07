using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Shipping.Methods;

public sealed class ShippingMethodsIntegrationTests(ApiFixture fixture) : ShippingIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ListShippingMethods_WithAuth_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            "/api/storefront/shipping/methods");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListShippingMethods_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/shipping/methods");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
