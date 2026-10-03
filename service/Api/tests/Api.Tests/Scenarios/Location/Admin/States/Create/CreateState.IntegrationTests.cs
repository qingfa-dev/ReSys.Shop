using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Location.Features.Shared.Countries.Models;
using Module.Location.Features.Shared.States.Models;

namespace Api.Tests.Scenarios.Location.Admin.States.Create;

public sealed class CreateStateIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    private async Task<Guid> GetFirstCountryIdAsync()
    {
        HttpResponseMessage allResponse = await Client.GetAsync("/api/locations/countries?pageSize=100");
        PagedResult<CountryListItemResponse> allResult = await allResponse.ReadAsPagedResultAsync<CountryListItemResponse>();
        string id = allResult.Items.First().Id.ToString();
        return Guid.Parse(id);
    }

    [Fact]
    public async Task CreateState_WithValidRequest_Returns200()
    {
        Guid countryId = await GetFirstCountryIdAsync();

        var request = new
        {
            name = "Test State",
            abbreviation = "TS",
            countryId = countryId,
            isActive = true
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/locations/states", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        StateListResponse? value = result.DeserializeValue<StateListResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Test State");
        value.Abbreviation.Should().Be("TS");
        value.CountryId.Should().Be(countryId);
    }

    [Fact]
    public async Task CreateState_WithDuplicateAbbreviation_Returns409()
    {
        Guid countryId = await GetFirstCountryIdAsync();

        var request = new
        {
            name = "Duplicate State",
            abbreviation = "TS",
            countryId = countryId,
            isActive = true
        };

        await Client.PostAsAdminRawAsync("/api/locations/states", request);

        HttpResponseMessage response = await Client.PostAsAdminRawAsync("/api/locations/states", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateState_WithNonexistentCountryId_Returns404()
    {
        Guid nonexistentCountryId = Guid.NewGuid();

        var request = new
        {
            name = "Orphan State",
            abbreviation = "OS",
            countryId = nonexistentCountryId,
            isActive = true
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/locations/states", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateState_WithMissingRequiredFields_Returns422()
    {
        var request = new
        {
            abbreviation = "XX"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/locations/states", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
