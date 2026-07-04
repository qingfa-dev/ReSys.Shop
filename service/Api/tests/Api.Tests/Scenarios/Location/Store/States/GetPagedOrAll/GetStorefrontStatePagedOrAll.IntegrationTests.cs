using Api.Tests.Infrastructure;

using Module.Location.Features.Admin.States.Shared.Models;

namespace Api.Tests.Scenarios.Location.Store.States.GetPagedOrAll;

public sealed class GetStorefrontStatePagedOrAllIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetStatePagedOrAll_ReturnsSeededStates()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/store/locations/states");
        PagedResult<StateListResponse> result = await response.ReadAsPagedResultAsync<StateListResponse>();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetStatePagedOrAll_WithPagination_RespectsPageSize()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/locations/states?pageSize=5");
        PagedResult<StateListResponse> result = await response.ReadAsPagedResultAsync<StateListResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCountLessThanOrEqualTo(5);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetStatePagedOrAll_WithFilter_IsActiveEqualsTrue()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/locations/states?filter=IsActive=true");
        PagedResult<StateListResponse> result = await response.ReadAsPagedResultAsync<StateListResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.All(i => i.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetStatePagedOrAll_WithSortAscending_ReturnsOrdered()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/locations/states?sort=Name:asc");
        PagedResult<StateListResponse> result = await response.ReadAsPagedResultAsync<StateListResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.First().Name.Should().Be("Alabama");
    }
}
