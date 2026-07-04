using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Identity.Store.Emails.Confirm;

public sealed class ConfirmEmailIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_Returns422()
    {
        var request = new
        {
            userId = Guid.NewGuid().ToString(),
            token = "invalid-token"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/emails/confirm", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ConfirmEmail_WithNonexistentUser_Returns404()
    {
        var request = new
        {
            userId = Guid.NewGuid().ToString(),
            token = "some-token"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/emails/confirm", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmEmail_WithMissingUserId_Returns404()
    {
        var request = new
        {
            token = "some-token"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/emails/confirm", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmEmail_WithMissingToken_Returns422()
    {
        var request = new
        {
            userId = Guid.NewGuid().ToString()
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/emails/confirm", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
