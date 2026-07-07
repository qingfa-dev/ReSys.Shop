using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Payment.Methods;

public sealed class PaymentMethodsIntegrationTests(ApiFixture fixture) : PaymentIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ListPaymentMethods_WithAuth_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            "/api/storefront/payment/methods");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListPaymentMethods_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/payment/methods");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
