using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Permissions.Get;

public sealed class GetUserPermissionsIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetUserPermissions_WithExistingUser_Returns200()
    {
        (Guid userId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, $"/api/identity/users/{userId}/permissions");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.ValueRaw.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserPermissions_WithNonexistentUser_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, $"/api/identity/users/{nonexistentId}/permissions");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserPermissions_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/identity/users/{someId}/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
