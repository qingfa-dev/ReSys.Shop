using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Identity.Store.Auth.Logout;

public sealed class LogoutIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Logout_WithoutAuth_Returns401()
    {
        var request = new { };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/auth/logout", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithMissingToken_Returns401()
    {
        var request = new
        {
            refreshToken = ""
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/auth/logout", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
