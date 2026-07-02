using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Identity.Store.Auth.External.Login;

public sealed class ExternalLoginIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ExternalLogin_WithMissingProvider_Returns422()
    {
        var request = new
        {
            provider = "",
            idToken = "some-token"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/login/external", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ExternalLogin_WithMissingIdToken_Returns422()
    {
        var request = new
        {
            provider = "google",
            idToken = ""
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/login/external", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
