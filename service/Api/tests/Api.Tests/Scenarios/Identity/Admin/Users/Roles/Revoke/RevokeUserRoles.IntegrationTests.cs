using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Roles.Revoke;

public sealed class RevokeUserRolesIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RevokeUserRoles_WithExistingRole_Returns200()
    {
        (Guid userId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);
        (Guid roleId, string roleName) = await IdentityTestHelper.CreateTestRoleAsync(Client);

        var assignRequest = new
        {
            roles = new[] { roleName }
        };

        using HttpRequestMessage assignRequestMsg = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Post, $"/api/identity/users/{userId}/roles/assign",
            JsonContent.Create(assignRequest));

        await Client.SendAsync(assignRequestMsg);

        var revokeRequest = new
        {
            roles = new[] { roleName }
        };

        using HttpRequestMessage revokeRequestMsg = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Post, $"/api/identity/users/{userId}/roles/revoke",
            JsonContent.Create(revokeRequest));

        HttpResponseMessage response = await Client.SendAsync(revokeRequestMsg);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RevokeUserRoles_WithNonexistentUser_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            roles = new[] { "SomeRole" }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Post, $"/api/identity/users/{nonexistentId}/roles/revoke",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RevokeUserRoles_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            roles = new[] { "SomeRole" }
        };

        using HttpRequestMessage noAuthRequest = new(
            HttpMethod.Post, $"/api/identity/users/{someId}/roles/revoke")
        {
            Content = JsonContent.Create(request)
        };

        HttpResponseMessage response = await Client.SendAsync(noAuthRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
