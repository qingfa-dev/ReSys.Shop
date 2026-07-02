using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Store.Passwords.Change;

public sealed class ChangePasswordIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ChangePassword_WithoutAuth_Returns401()
    {
        var request = new
        {
            currentPassword = "OldPass123!",
            newPassword = "NewPass1234!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/passwords/change", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithMissingCurrentPassword_Returns422()
    {
        (Guid userId, string email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            newPassword = "NewPass1234!"
        };

        using HttpRequestMessage httpRequest = IdentityTestHelper.CreateUserRequest(
            HttpMethod.Post, "/api/store/identity/passwords/change", userId, email,
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(httpRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ChangePassword_WithMissingNewPassword_Returns422()
    {
        (Guid userId, string email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            currentPassword = "OldPass123!"
        };

        using HttpRequestMessage httpRequest = IdentityTestHelper.CreateUserRequest(
            HttpMethod.Post, "/api/store/identity/passwords/change", userId, email,
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(httpRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ChangePassword_WithWeakNewPassword_Returns422()
    {
        (Guid userId, string email, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            currentPassword = "OldPass123!",
            newPassword = "weak"
        };

        using HttpRequestMessage httpRequest = IdentityTestHelper.CreateUserRequest(
            HttpMethod.Post, "/api/store/identity/passwords/change", userId, email,
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(httpRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
