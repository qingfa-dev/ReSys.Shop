using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Profiles.Delete;

public sealed class DeleteProfileIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteProfile_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.DeleteAsync(
            "/api/store/profiles/profiles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
