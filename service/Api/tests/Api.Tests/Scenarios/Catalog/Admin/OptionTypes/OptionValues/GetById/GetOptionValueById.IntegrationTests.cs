using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.OptionValues.GetById;

public sealed class GetOptionValueByIdIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetOptionValueById_WithCreatedOptionValue_Returns200()
    {
        var createOptionTypeRequest = new
        {
            name = "Finish",
            presentation = "Finish",
            position = 1,
            filterable = true
        };

        HttpResponseMessage createOtResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/option-types", createOptionTypeRequest);
        ApiResponse createOtResult = await createOtResponse.ReadApiResponseAsync();
        createOtResult.IsSuccess.Should().BeTrue();
        OptionTypeDetailResponse? optionType = createOtResult.DeserializeValue<OptionTypeDetailResponse>();
        optionType.Should().NotBeNull();

        var createValueRequest = new
        {
            name = "Matte",
            presentation = "Matte",
            optionTypeId = optionType!.Id
        };

        HttpResponseMessage createValResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/option-types/option-values", createValueRequest);
        ApiResponse createValResult = await createValResponse.ReadApiResponseAsync();
        createValResult.IsSuccess.Should().BeTrue();
        OptionValueListItemResponse? created = createValResult.DeserializeValue<OptionValueListItemResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/option-types/option-values/{created!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        OptionValueListItemResponse? value = result.DeserializeValue<OptionValueListItemResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Matte");
        value.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetOptionValueById_WithNonexistentId_Returns404()
    {
        Guid optionTypeId = Guid.NewGuid();
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/option-types/option-values/{nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
