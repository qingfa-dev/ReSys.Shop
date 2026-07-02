using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Identity.Store.Auth.Sessions.Refresh;

public sealed class RefreshSessionIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RefreshSession_WithMissingToken_Returns422()
    {
        var request = new
        {
            refreshToken = ""
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/sessions/refresh", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RefreshSession_WithInvalidToken_Returns404()
    {
        var request = new
        {
            refreshToken = "invalid-refresh-token"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/sessions/refresh", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
