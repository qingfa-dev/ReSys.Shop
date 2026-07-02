using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Roles.Permissions.Get;

public sealed class GetRolePermissionsIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetRolePermissions_WithExistingRole_Returns200()
    {
        (Guid roleId, _) = await IdentityTestHelper.CreateTestRoleAsync(Client);

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, $"/api/identity/roles/{roleId}/permissions");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.ValueRaw.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRolePermissions_WithNonexistentRole_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, $"/api/identity/roles/{nonexistentId}/permissions");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRolePermissions_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/identity/roles/{someId}/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
