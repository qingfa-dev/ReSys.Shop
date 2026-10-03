using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using Module.Identity.Features.Admin.Shared.Models;

namespace Api.Tests.Scenarios.Identity.Admin.Roles.Update;

public sealed class UpdateRoleIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateRole_WithValidRequest_Returns200()
    {
        (Guid roleId, _) = await IdentityTestHelper.CreateTestRoleAsync(Client);

        var request = new
        {
            name = $"updated{Guid.NewGuid():N}"[..15],
            description = "Updated description"
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Put, $"/api/identity/roles/{roleId}",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        RoleDetailResponse? value = result.DeserializeValue<RoleDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be((string)request.name);
        value.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateRole_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            name = $"ghost{Guid.NewGuid():N}"[..10],
            description = "Ghost role"
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Put, $"/api/identity/roles/{nonexistentId}",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRole_WithDuplicateName_Returns409()
    {
        (Guid roleId1, string roleName1) = await IdentityTestHelper.CreateTestRoleAsync(Client);
        (_, string roleName2) = await IdentityTestHelper.CreateTestRoleAsync(Client);

        var request = new
        {
            name = roleName2
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Put, $"/api/identity/roles/{roleId1}",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateRole_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            name = "UpdatedRole"
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"/api/identity/roles/{someId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
