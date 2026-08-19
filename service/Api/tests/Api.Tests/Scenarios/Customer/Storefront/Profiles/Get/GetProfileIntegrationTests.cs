using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Profiles.Get;

public sealed class GetProfileIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetProfile_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/storefront/profiles/profiles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
