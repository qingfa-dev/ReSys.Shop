using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Permissions.Assign;

public sealed class AssignUserPermissionsIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AssignUserPermissions_WithNonexistentUser_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Post, $"/api/identity/users/{nonexistentId}/permissions/assign",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignUserPermissions_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            permissions = new[] { "system.users.list" }
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/identity/users/{someId}/permissions/assign", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
