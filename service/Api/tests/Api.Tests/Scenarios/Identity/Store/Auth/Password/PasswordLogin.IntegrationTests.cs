using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Identity.Store.Auth.Password;

public sealed class PasswordLoginIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task PasswordLogin_WithInvalidCredentials_Returns401()
    {
        var request = new
        {
            credential = "nonexistent@example.com",
            password = "WrongPass123!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/auth/login/password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PasswordLogin_WithMissingCredential_Returns422()
    {
        var request = new
        {
            password = "SomePass123!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/auth/login/password", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PasswordLogin_WithMissingPassword_Returns422()
    {
        var request = new
        {
            credential = "test@example.com"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/auth/login/password", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PasswordLogin_WithEmptyBody_Returns422()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/auth/login/password", new { });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
