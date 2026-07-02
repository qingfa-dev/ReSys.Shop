using System.Net;

using Api.Tests.Infrastructure;

using Module.Location.Features.Admin.States.Shared.Models;

namespace Api.Tests.Scenarios.Location.Admin.States.Update;

public sealed class UpdateStateIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    private async Task<(string StateId, string CountryId)> GetFirstStateAsync()
    {
        HttpResponseMessage allResponse = await Client.GetAsync("/api/locations/states?pageSize=100");
        PagedResult<StateListResponse> allResult = await allResponse.ReadAsPagedResultAsync<StateListResponse>();
        StateListResponse first = allResult.Items.First();
        return (first.Id.ToString(), first.CountryId.ToString());
    }

    [Fact]
    public async Task UpdateState_WithValidRequest_Returns200()
    {
        (string stateId, string countryId) = await GetFirstStateAsync();

        var request = new
        {
            name = "Updated State",
            abbreviation = "US",
            countryId = Guid.Parse(countryId),
            isActive = true
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/locations/states/{stateId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        StateDetailResponse? value = result.DeserializeValue<StateDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Updated State");
        value.Abbreviation.Should().Be("US");
    }

    [Fact]
    public async Task UpdateState_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            name = "Ghost State",
            abbreviation = "GS",
            countryId = Guid.NewGuid(),
            isActive = true
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/locations/states/{nonexistentId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateState_WithDuplicateAbbreviation_Returns409()
    {
        (string firstStateId, string countryId) = await GetFirstStateAsync();
        Guid parsedCountryId = Guid.Parse(countryId);

        var createRequest = new
        {
            name = "Second State",
            abbreviation = "SS",
            countryId = parsedCountryId,
            isActive = true
        };
        HttpResponseMessage createResponse = await Client.PostAsJsonAsync("/api/locations/states", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        StateListResponse? created = createResult.DeserializeValue<StateListResponse>();
        created.Should().NotBeNull();
        string duplicateAbbreviation = created!.Abbreviation;

        var updateRequest = new
        {
            name = "Should Fail",
            abbreviation = duplicateAbbreviation,
            countryId = parsedCountryId,
            isActive = true
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/locations/states/{firstStateId}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateState_WithNonexistentCountryId_Returns404()
    {
        (string stateId, _) = await GetFirstStateAsync();
        Guid nonexistentCountryId = Guid.NewGuid();

        var request = new
        {
            name = "Orphan State",
            abbreviation = "OS",
            countryId = nonexistentCountryId,
            isActive = true
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/locations/states/{stateId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateState_WithMissingRequiredFields_Returns422()
    {
        (string stateId, _) = await GetFirstStateAsync();

        var request = new
        {
            abbreviation = ""
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/locations/states/{stateId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
