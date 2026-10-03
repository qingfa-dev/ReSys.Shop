using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Store.Emails.Change;

public sealed class ChangeEmailIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ChangeEmail_WithoutAuth_Returns401()
    {
        var request = new
        {
            newEmail = "newemail@example.com",
            password = "SomePass123!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/storefront/identity/emails/change", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeEmail_WithMissingNewEmail_Returns422()
    {
        (Guid userId, string email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            password = "SomePass123!"
        };

        using HttpRequestMessage httpRequest = IdentityTestHelper.CreateUserRequest(
            HttpMethod.Post, "/api/storefront/identity/emails/change", userId, email,
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(httpRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ChangeEmail_WithMissingPassword_Returns422()
    {
        (Guid userId, string email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            newEmail = "newemail@example.com"
        };

        using HttpRequestMessage httpRequest = IdentityTestHelper.CreateUserRequest(
            HttpMethod.Post, "/api/storefront/identity/emails/change", userId, email,
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(httpRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ChangeEmail_WithInvalidEmail_Returns422()
    {
        (Guid userId, string email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            newEmail = "not-an-email",
            password = "SomePass123!"
        };

        using HttpRequestMessage httpRequest = IdentityTestHelper.CreateUserRequest(
            HttpMethod.Post, "/api/storefront/identity/emails/change", userId, email,
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(httpRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
