using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Location.Features.Shared.Countries.Models;
using Module.Location.Features.Shared.States.Models;

namespace Api.Tests.Scenarios.Location.Admin.States.Delete;

public sealed class DeleteStateIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    private async Task<Guid> GetFirstCountryIdAsync()
    {
        HttpResponseMessage allResponse = await Client.GetAsync("/api/locations/countries?pageSize=100");
        PagedResult<CountryListItemResponse> allResult = await allResponse.ReadAsPagedResultAsync<CountryListItemResponse>();
        string id = allResult.Items.First().Id.ToString();
        return Guid.Parse(id);
    }

    [Fact]
    public async Task DeleteState_WithExistingId_Returns204()
    {
        Guid countryId = await GetFirstCountryIdAsync();

        var createRequest = new
        {
            name = "Deletable State",
            abbreviation = "DS",
            countryId = countryId,
            isActive = true
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/locations/states", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        StateListResponse? created = createResult.DeserializeValue<StateListResponse>();
        created.Should().NotBeNull();
        Guid newId = created!.Id;

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/locations/states/{newId}");

        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteState_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/locations/states/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
