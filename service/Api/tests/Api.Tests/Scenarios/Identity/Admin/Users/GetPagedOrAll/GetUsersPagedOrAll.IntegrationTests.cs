using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

using Module.Identity.Features.Shared.Admin.Users.Shared.Models;

namespace Api.Tests.Scenarios.Identity.Admin.Users.GetPagedOrAll;

public sealed class GetUsersPagedOrAllIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetUsersPagedOrAll_ReturnsSeededUsers()
    {
        await IdentityTestHelper.CreateTestUserAsync(Client);

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, "/api/identity/users");
        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        PagedResult<UserListResponse> result = await response.ReadAsPagedResultAsync<UserListResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetUsersPagedOrAll_WithPagination_RespectsPageSize()
    {
        await IdentityTestHelper.CreateTestUserAsync(Client);
        await IdentityTestHelper.CreateTestUserAsync(Client);

        using HttpRequestMessage adminRequest = IdentityTestHelper.CreateAdminRequest(
            HttpMethod.Get, "/api/identity/users?pageSize=1");
        HttpResponseMessage response = await Client.SendAsync(adminRequest);
        PagedResult<UserListResponse> result = await response.ReadAsPagedResultAsync<UserListResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCountLessThanOrEqualTo(1);
        result.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task GetUsersPagedOrAll_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/identity/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
