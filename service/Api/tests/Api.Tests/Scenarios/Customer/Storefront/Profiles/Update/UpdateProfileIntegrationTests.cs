using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Profile.Store.Profiles.Update;

public sealed class UpdateProfileIntegrationTests(ApiFixture fixture) : ProfileIntegrationTestBase(fixture)
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
            "/api/storefront/profiles/profiles", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
