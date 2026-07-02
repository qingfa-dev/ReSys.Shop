using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Api.Tests.Scenarios.Identity.Admin.Roles.GetById;

public sealed class GetRoleByIdIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetRoleById_WithExistingRole_Returns200()
    {
        (Guid roleId, string roleName) = await IdentityTestHelper.CreateTestRoleAsync(Client);

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, $"/api/identity/roles/{roleId}");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        RoleDetailResponse? value = result.DeserializeValue<RoleDetailResponse>();
        value.Should().NotBeNull();
        value!.Id.Should().Be(roleId);
        value.Name.Should().Be(roleName);
    }

    [Fact]
    public async Task GetRoleById_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, $"/api/identity/roles/{nonexistentId}");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRoleById_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync($"/api/identity/roles/{someId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
