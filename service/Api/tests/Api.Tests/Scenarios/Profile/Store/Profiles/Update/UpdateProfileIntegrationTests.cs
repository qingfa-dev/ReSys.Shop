using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Profiles.Update;

public sealed class UpdateProfileIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateProfile_WithoutAuth_Returns401()
    {
        var request = new
        {
            firstName = "Test",
            lastName = "User"
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            "/api/store/profiles/profiles", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
