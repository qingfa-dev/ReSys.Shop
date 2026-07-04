using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Roles.Permissions.Assign;

public sealed class AssignRolePermissionsIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AssignRolePermissions_WithNonexistentRole_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Put, $"/api/identity/roles/{nonexistentId}/permissions/assign",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignRolePermissions_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        using HttpRequestMessage noAuthRequest = new HttpRequestMessage(
            HttpMethod.Put, $"/api/identity/roles/{someId}/permissions/assign")
        {
            Content = JsonContent.Create(request)
        };

        HttpResponseMessage response = await Client.SendAsync(noAuthRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
