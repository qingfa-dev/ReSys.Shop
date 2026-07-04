using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Status;

public sealed class ToggleUserStatusIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task ToggleUserStatus_WithExistingUser_Returns200()
    {
        (Guid userId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Patch, $"/api/identity/users/{userId}/status");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ToggleUserStatus_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Patch, $"/api/identity/users/{nonexistentId}/status");

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleUserStatus_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        HttpResponseMessage response = await Client.PatchAsync(
            $"/api/identity/users/{someId}/status", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
