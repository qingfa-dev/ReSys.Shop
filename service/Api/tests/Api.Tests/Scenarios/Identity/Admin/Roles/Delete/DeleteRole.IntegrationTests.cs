using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Roles.Delete;

public sealed class DeleteRoleIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteRole_WithExistingId_ReturnsSuccess()
    {
        (Guid roleId, _) = await IdentityTestHelper.CreateTestRoleAsync(Client);

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/identity/roles/{roleId}");

        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRole_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/identity/roles/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRole_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/identity/roles/{someId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
