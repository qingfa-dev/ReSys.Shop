using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Identity.Store.Auth.External.Providers;

public sealed class ExternalProvidersIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetExternalProviders_Returns200()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/identity/auth/login/external/providers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
