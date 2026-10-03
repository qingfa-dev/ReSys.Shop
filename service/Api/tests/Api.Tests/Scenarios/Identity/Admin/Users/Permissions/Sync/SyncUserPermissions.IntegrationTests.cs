using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Permissions.Sync;

public sealed class SyncUserPermissionsIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task SyncUserPermissions_WithNonexistentUser_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Put, $"/api/identity/users/{nonexistentId}/permissions/sync",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SyncUserPermissions_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        using HttpRequestMessage noAuthRequest = new(
            HttpMethod.Put, $"/api/identity/users/{someId}/permissions/sync")
        {
            Content = JsonContent.Create(request)
        };

        HttpResponseMessage response = await Client.SendAsync(noAuthRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
