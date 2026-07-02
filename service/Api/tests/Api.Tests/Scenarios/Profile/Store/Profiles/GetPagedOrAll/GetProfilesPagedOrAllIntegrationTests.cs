using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Profiles.GetPagedOrAll;

public sealed class GetProfilesPagedOrAllIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetProfilesAll_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/profiles/profiles/all");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
