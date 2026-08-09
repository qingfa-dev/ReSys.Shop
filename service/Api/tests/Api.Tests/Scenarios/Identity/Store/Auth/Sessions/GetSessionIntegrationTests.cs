using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Identity.Store.Auth.Sessions;

public sealed class GetSessionIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetSession_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/identity/auth/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
