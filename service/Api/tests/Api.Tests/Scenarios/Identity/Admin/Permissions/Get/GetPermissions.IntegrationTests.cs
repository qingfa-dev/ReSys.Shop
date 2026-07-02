using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using Shared.Security.Identity.Domain.Permissions;

namespace Api.Tests.Scenarios.Identity.Admin.Permissions.Get;

public sealed class GetPermissionsIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetPermissions_ReturnsAllSystemPermissions()
    {
        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, "/api/identity/permissions");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        PagedResult<PermissionMetadata> result = await response.ReadAsPagedResultAsync<PermissionMetadata>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPermissions_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/identity/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
