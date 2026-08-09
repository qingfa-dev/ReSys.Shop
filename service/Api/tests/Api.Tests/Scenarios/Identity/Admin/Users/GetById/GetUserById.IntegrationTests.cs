using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using Module.Identity.Features.Shared.Admin.Users.Shared.Models;

namespace Api.Tests.Scenarios.Identity.Admin.Users.GetById;

public sealed class GetUserByIdIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetUserById_WithExistingUser_Returns200()
    {
        (Guid userId, string email, string userName) = await IdentityTestHelper.CreateTestUserAsync(Client);

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, $"/api/identity/users/{userId}");
        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        UserDetailResponse? value = result.DeserializeValue<UserDetailResponse>();
        value.Should().NotBeNull();
        value!.Id.Should().Be(userId);
        value.Email.Should().Be(email);
        value.UserName.Should().Be(userName);
    }

    [Fact]
    public async Task GetUserById_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, $"/api/identity/users/{nonexistentId}");
        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserById_WithoutAuth_Returns401()
    {
        Guid someId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync($"/api/identity/users/{someId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
