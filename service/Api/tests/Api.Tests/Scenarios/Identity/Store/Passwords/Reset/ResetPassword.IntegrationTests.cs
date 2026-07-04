using System.Net;

using Api.Tests.Infrastructure;

namespace Api.Tests.Scenarios.Identity.Store.Passwords.Reset;

public sealed class ResetPasswordIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ResetPassword_WithInvalidToken_Returns422()
    {
        var request = new
        {
            userId = Guid.NewGuid().ToString(),
            token = "invalid-token",
            newPassword = "NewValidPass1!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/reset", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ResetPassword_WithNonexistentUser_Returns422()
    {
        var request = new
        {
            userId = Guid.NewGuid().ToString(),
            token = "some-valid-looking-token",
            newPassword = "NewValidPass1!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/reset", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ResetPassword_WithMissingUserId_Returns422()
    {
        var request = new
        {
            token = "some-token",
            newPassword = "NewValidPass1!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/reset", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ResetPassword_WithMissingToken_Returns422()
    {
        var request = new
        {
            userId = Guid.NewGuid().ToString(),
            newPassword = "NewValidPass1!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/reset", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ResetPassword_WithWeakPassword_Returns422()
    {
        var request = new
        {
            userId = Guid.NewGuid().ToString(),
            token = "some-token",
            newPassword = "weak"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/reset", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
