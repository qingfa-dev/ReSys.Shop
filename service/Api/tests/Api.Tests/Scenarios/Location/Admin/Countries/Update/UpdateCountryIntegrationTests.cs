using System.Net;

using Api.Tests.Infrastructure;

using Module.Location.Features.Admin.Countries.Shared.Models;

namespace Api.Tests.Scenarios.Locations.Admin.Countries.Update;

public sealed class UpdateCountryIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    private async Task<string> GetFirstCountryIdAsync()
    {
        HttpResponseMessage allResponse = await Client.GetAsync("/api/locations/countries?pageSize=100");
        PagedResult<CountryListItemResponse> allResult = await allResponse.ReadAsPagedResultAsync<CountryListItemResponse>();
        return allResult.Items.First().Id.ToString();
    }

    [Fact]
    public async Task UpdateCountry_WithValidRequest_Returns200()
    {
        string firstId = await GetFirstCountryIdAsync();

        var request = new
        {
            name = "Updated Country",
            isoCode = "UC",
            callingCode = "+00",
            statesRequired = false,
            isActive = true
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/locations/countries/{firstId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        CountryDetailResponse? value = result.DeserializeValue<CountryDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Updated Country");
        value.IsoCode.Should().Be("UC");
    }

    [Fact]
    public async Task UpdateCountry_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            name = "Ghost Country",
            isoCode = "GC",
            statesRequired = false,
            isActive = true
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/locations/countries/{nonexistentId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCountry_WithDuplicateIsoCode_Returns409()
    {
        HttpResponseMessage allResponse = await Client.GetAsync("/api/locations/countries?pageSize=100");
        PagedResult<CountryListItemResponse> allResult = await allResponse.ReadAsPagedResultAsync<CountryListItemResponse>();
        List<CountryListItemResponse> items = allResult.Items.ToList();
        string firstId = items[0].Id.ToString();
        string secondIsoCode = items[1].IsoCode.ToString();

        var request = new
        {
            name = "Will Fail",
            isoCode = secondIsoCode,
            statesRequired = false,
            isActive = true
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/locations/countries/{firstId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateCountry_WithMissingRequiredFields_Returns422()
    {
        string firstId = await GetFirstCountryIdAsync();

        var request = new
        {
            isoCode = ""
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/locations/countries/{firstId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
