using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Location.Features.Shared.Countries.Models;

namespace Api.Tests.Scenarios.Location.Admin.Countries.Delete;

public sealed class DeleteCountryIntegrationTests(ApiFixture fixture) : LocationIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteCountry_WithExistingId_ReturnsSuccess()
    {
        var createRequest = new
        {
            name = "Deletable Country",
            isoCode = "DC",
            statesRequired = false,
            isActive = true
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/locations/countries", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        CountryListItemResponse? created = createResult.DeserializeValue<CountryListItemResponse>();
        created.Should().NotBeNull();
        Guid newId = created!.Id;

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/locations/countries/{newId}");

        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCountry_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/locations/countries/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCountry_WithoutAuth_Returns401()
    {
        HttpResponseMessage allResponse = await Client.GetAsync("/api/locations/countries?pageSize=100");
        PagedResult<CountryListItemResponse> allResult = await allResponse.ReadAsPagedResultAsync<CountryListItemResponse>();
        string firstId = allResult.Items.First().Id.ToString();

        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/locations/countries/{firstId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
