using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Store.Emails.Resend;

public sealed class ResendEmailVerificationIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ResendVerification_WithValidEmail_Returns204()
    {
        (Guid userId, string email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new { email };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/emails/resend", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ResendVerification_WithNonexistentEmail_Returns204()
    {
        var request = new { email = "nonexistent@example.com" };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/emails/resend", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ResendVerification_WithMissingEmail_Returns422()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/emails/resend", new { });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
