using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Store.Passwords.Forgot;

public sealed class RequestPasswordResetIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RequestPasswordReset_WithValidEmail_Returns204()
    {
        (Guid userId, string email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new { email };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/forgot", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RequestPasswordReset_WithNonexistentEmail_Returns204()
    {
        var request = new { email = "nonexistent@example.com" };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/forgot", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RequestPasswordReset_WithMissingEmail_Returns204()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/forgot", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
