using System.Text.Json;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Storefront.OptionTypes.GetAll;

public sealed class GetAllOptionTypesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record OptionTypeIdResponse
    {
        public string Id { get; init; } = "";
    }

    [Fact]
    public async Task GetAllOptionTypes_WithExistingTypes_ReturnsItems()
    {
        var createOptionTypeRequest = new
        {
            name = "Storefront Color",
            presentation = "Color",
            position = 1,
            filterable = true
        };
        HttpResponseMessage createOptionTypeResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/option-types", createOptionTypeRequest);
        ApiResponse createOptionTypeResult = await createOptionTypeResponse.ReadApiResponseAsync();
        createOptionTypeResult.IsSuccess.Should().BeTrue();
        string optionTypeId = createOptionTypeResult.DeserializeValue<OptionTypeIdResponse>()!.Id;

        var createValueRequest = new
        {
            name = "Red",
            presentation = "Red"
        };
        HttpResponseMessage createValueResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/option-types/{optionTypeId}/values", createValueRequest);
        createValueResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsync("/api/storefront/option-types");
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllOptionTypes_WithoutData_ReturnsEmptyList()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/storefront/option-types");
        PagedResult<JsonElement> result = await response.ReadAsPagedResultAsync<JsonElement>();

        result.IsSuccess.Should().BeTrue();
    }
}
