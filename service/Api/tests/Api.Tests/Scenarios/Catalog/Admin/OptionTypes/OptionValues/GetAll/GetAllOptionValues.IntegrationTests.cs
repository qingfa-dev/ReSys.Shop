using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.OptionValues.GetAll;

public sealed class GetAllOptionValuesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAllOptionValues_ReturnsOptionValues()
    {
        var createOptionTypeRequest = new
        {
            name = "TestSize",
            presentation = "TestSize",
            position = 1,
            filterable = true
        };

        HttpResponseMessage createOtResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/option-types", createOptionTypeRequest);
        ApiResponse createOtResult = await createOtResponse.ReadApiResponseAsync();
        createOtResult.IsSuccess.Should().BeTrue();
        OptionTypeDetailResponse? optionType = createOtResult.DeserializeValue<OptionTypeDetailResponse>();
        optionType.Should().NotBeNull();
        Guid optionTypeId = optionType!.Id;

        var createValueRequest = new
        {
            name = "Small",
            presentation = "Small"
        };

        HttpResponseMessage createValResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/option-types/{optionTypeId}/values", createValueRequest);
        createValResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/option-types/{optionTypeId}/values?pageSize=100");
        PagedResult<OptionValueListItemResponse> result = await response.ReadAsPagedResultAsync<OptionValueListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.Should().Contain(v => v.Name == "Small");
    }
}
