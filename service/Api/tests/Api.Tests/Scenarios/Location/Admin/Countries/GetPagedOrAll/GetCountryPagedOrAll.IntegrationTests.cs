using Api.Tests.Infrastructure;

using Module.Location.Features.Shared.Countries.Models;

namespace Api.Tests.Scenarios.Location.Admin.Countries.GetPagedOrAll;

public sealed class GetCountryPagedOrAllIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetCountryPagedOrAll_ReturnsSeededCountries()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/locations/countries");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithPageSize_ReturnsCorrectCount()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/locations/countries?pageSize=1");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCountLessThanOrEqualTo(1);
        result.PageSize.Should().Be(1);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithMaxPageSize_ReturnsAllItems()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/locations/countries?pageSize=100");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithFilter_IsActiveEqualsTrue()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/locations/countries?filter=IsActive=true");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.All(i => i.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithFilter_StatesRequiredEqualsTrue()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/locations/countries?filter=StatesRequired=true");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.All(i => i.StatesRequired).Should().BeTrue();
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithSortAscending_ReturnsOrdered()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/locations/countries?sort=Name:asc");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        List<CountryListItemResponse> items = result.Items.ToList();
        items.Should().BeInAscendingOrder(i => i.Name);
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithSortDescending_ReturnsOrdered()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/locations/countries?sort=Name:desc");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        List<CountryListItemResponse> items = result.Items.ToList();
        items.Should().BeInDescendingOrder(i => i.Name);
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithFilter_IsoCodeEquals_ReturnsFiltered()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/locations/countries?filter=IsoCode=US");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().ContainSingle();
        result.Items.First().IsoCode.Should().Be("US");
    }

    [Fact]
    public async Task GetCountryPagedOrAll_WithSearch_ReturnsMatching()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/locations/countries?search=Vietnam&searchFields=Name");
        PagedResult<CountryListItemResponse> result = await response.ReadAsPagedResultAsync<CountryListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.First().Name.Should().Contain("Vietnam");
    }
}
