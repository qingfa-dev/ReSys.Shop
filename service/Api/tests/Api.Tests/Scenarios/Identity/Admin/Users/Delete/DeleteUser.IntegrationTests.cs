using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Delete;

public sealed class DeleteUserIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteUser_WithExistingId_ReturnsSuccess()
    {
        (Guid userId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/identity/users/{userId}");

        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUser_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/identity/users/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/identity/users/{someId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
