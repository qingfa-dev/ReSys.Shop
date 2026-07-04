using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Roles.Assign;

public sealed class AssignUserRolesIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AssignUserRoles_WithValidRole_Returns200()
    {
        (Guid userId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);
        (Guid roleId, string roleName) = await IdentityTestHelper.CreateTestRoleAsync(Client);

        var request = new
        {
            roles = new[] { roleName }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Post, $"/api/identity/users/{userId}/roles/assign",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AssignUserRoles_WithNonexistentUser_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();
        (Guid roleId, string roleName) = await IdentityTestHelper.CreateTestRoleAsync(Client);

        var request = new
        {
            roles = new[] { roleName }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Post, $"/api/identity/users/{nonexistentId}/roles/assign",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignUserRoles_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            roles = new[] { "SomeRole" }
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/identity/users/{someId}/roles/assign", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
