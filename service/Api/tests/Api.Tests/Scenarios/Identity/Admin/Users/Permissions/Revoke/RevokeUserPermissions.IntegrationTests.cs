using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Permissions.Revoke;

public sealed class RevokeUserPermissionsIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RevokeUserPermissions_WithNonexistentUser_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Delete, $"/api/identity/users/{nonexistentId}/permissions/revoke",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RevokeUserPermissions_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        using HttpRequestMessage noAuthRequest = new HttpRequestMessage(
            HttpMethod.Delete, $"/api/identity/users/{someId}/permissions/revoke")
        {
            Content = JsonContent.Create(request)
        };

        HttpResponseMessage response = await Client.SendAsync(noAuthRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
