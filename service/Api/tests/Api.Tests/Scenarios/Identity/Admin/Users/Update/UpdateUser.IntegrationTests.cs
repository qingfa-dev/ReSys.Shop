using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using Module.Identity.Features.Shared.Admin.Users.Shared.Models;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Update;

public sealed class UpdateUserIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateUser_WithValidRequest_Returns200()
    {
        (Guid userId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            id = userId,
            email = IdentityTestHelper.ValidEmail(),
            userName = IdentityTestHelper.ValidUserName(),
            firstName = "Updated",
            lastName = "Name"
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Put, $"/api/identity/users/{userId}",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        UserDetailResponse? value = result.DeserializeValue<UserDetailResponse>();
        value.Should().NotBeNull();
        value!.FirstName.Should().Be("Updated");
        value.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateUser_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            id = nonexistentId,
            email = IdentityTestHelper.ValidEmail(),
            userName = IdentityTestHelper.ValidUserName(),
            firstName = "Ghost",
            lastName = "User"
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Put, $"/api/identity/users/{nonexistentId}",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_WithDuplicateEmail_Returns409()
    {
        (Guid firstUserId, string firstEmail, _) = await IdentityTestHelper.CreateTestUserAsync(Client);
        (Guid secondUserId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            id = secondUserId,
            email = firstEmail,
            userName = IdentityTestHelper.ValidUserName(),
            firstName = "Conflict",
            lastName = "User"
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Put, $"/api/identity/users/{secondUserId}",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidFields_Returns422()
    {
        (Guid userId, _, _) = await IdentityTestHelper.CreateTestUserAsync(Client);

        var request = new
        {
            id = userId,
            email = "not-an-email",
            userName = "ab",
            firstName = "",
            lastName = ""
        };

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Put, $"/api/identity/users/{userId}",
            JsonContent.Create(request));

        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateUser_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        var request = new
        {
            email = "test@example.com",
            userName = "testuser",
            firstName = "Test",
            lastName = "User"
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"/api/identity/users/{someId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
