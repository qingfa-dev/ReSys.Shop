using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Roles.Permissions.Sync;

public sealed class SyncRolePermissionsIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task SyncRolePermissions_WithNonexistentRole_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Patch, $"/api/identity/roles/{nonexistentId}/permissions/sync",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SyncRolePermissions_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        using HttpRequestMessage noAuthRequest = new HttpRequestMessage(
            HttpMethod.Patch, $"/api/identity/roles/{someId}/permissions/sync")
        {
            Content = JsonContent.Create(request)
        };

        HttpResponseMessage response = await Client.SendAsync(noAuthRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
