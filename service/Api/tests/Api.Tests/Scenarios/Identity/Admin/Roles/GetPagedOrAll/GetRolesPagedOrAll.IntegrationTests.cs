using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using Module.Identity.Features.Shared.Admin.Roles.Shared.Models;

namespace Api.Tests.Scenarios.Identity.Admin.Roles.GetPagedOrAll;

public sealed class GetRolesPagedOrAllIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetRolesPagedOrAll_ReturnsRoles()
    {
        await IdentityTestHelper.CreateTestRoleAsync(Client);

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, "/api/identity/roles");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        PagedResult<RoleListResponse> result = await response.ReadAsPagedResultAsync<RoleListResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetRolesPagedOrAll_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/identity/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
