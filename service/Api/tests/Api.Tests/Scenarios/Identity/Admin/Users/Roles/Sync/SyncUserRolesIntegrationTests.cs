using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Roles.Sync;

public sealed class SyncUserRolesIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task SyncUserRoles_WithValidRoleSet_Returns200()
    {
        (Guid userId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);
        (Guid roleId1, string roleName1) = await IdentityTestHelper.CreateTestRoleAsync(Client);
        (Guid roleId2, string roleName2) = await IdentityTestHelper.CreateTestRoleAsync(Client);

        var request = new
        {
            roles = new[] { roleName1, roleName2 }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Patch, $"/api/identity/users/{userId}/roles/sync",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SyncUserRoles_WithEmptyRoles_ClearsAll()
    {
        (Guid userId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            roles = Array.Empty<string>()
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Patch, $"/api/identity/users/{userId}/roles/sync",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        var result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SyncUserRoles_WithNonexistentUser_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            roles = new[] { "SomeRole" }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Patch, $"/api/identity/users/{nonexistentId}/roles/sync",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SyncUserRoles_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            roles = new[] { "SomeRole" }
        };

        using HttpRequestMessage noAuthRequest = new HttpRequestMessage(
            HttpMethod.Patch, $"/api/identity/users/{someId}/roles/sync")
        {
            Content = JsonContent.Create(request)
        };

        HttpResponseMessage response = await Client.SendAsync(noAuthRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
