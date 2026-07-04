using System.Net;

using Api.Tests.Infrastructure;

using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Api.Tests.Scenarios.Location.Admin.Countries.GetById;

public sealed class GetCountryByIdIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetCountryById_WithSeededCountry_Returns200()
    {
        HttpResponseMessage allResponse = await Client.GetAsync("/api/locations/countries?pageSize=100");
        PagedResult<CountryListItemResponse> allResult = await allResponse.ReadAsPagedResultAsync<CountryListItemResponse>();
        string firstId = allResult.Items.First().Id.ToString();

        HttpResponseMessage response = await Client.GetAsync($"/api/locations/countries/{firstId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        CountryDetailResponse? value = result.DeserializeValue<CountryDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().NotBeNullOrEmpty();
        value.IsoCode.Should().NotBeNullOrEmpty();
        value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCountryById_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync($"/api/locations/countries/{nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
