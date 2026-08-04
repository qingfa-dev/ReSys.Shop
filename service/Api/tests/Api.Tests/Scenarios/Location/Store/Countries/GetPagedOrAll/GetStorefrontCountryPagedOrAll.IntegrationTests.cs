using Api.Tests.Infrastructure;

using Module.Location.Features.Shared.Countries.Models;

namespace Api.Tests.Scenarios.Location.Store.Countries.GetPagedOrAll;

public sealed class GetStorefrontCountryPagedOrAllIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetCountryPagedOrAll_ReturnsSeededCountries()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/store/locations/countries");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithPagination_RespectsPageSize()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/locations/countries?pageSize=1");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCountLessThanOrEqualTo(1);
        result.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithFilter_IsActiveEqualsTrue()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/locations/countries?filter=IsActive=true");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.All(i => i.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithSortAscending_ReturnsOrdered()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/store/locations/countries?sort=Name:asc");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        List<CountryListItemResponse> items = result.Items.ToList();
        items.Should().BeInAscendingOrder(i => i.Name);
    }
}
