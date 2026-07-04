using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;
using Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.OptionValues.Update;

public sealed class UpdateOptionValueIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateOptionValue_WithValidRequest_Returns200()
    {
        var createOptionTypeRequest = new
        {
            name = "Texture",
            presentation = "Texture",
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
            name = "Smooth",
            presentation = "Smooth"
        };

        HttpResponseMessage createValResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/option-types/{optionType!.Id}/values", createValueRequest);
        ApiResponse createValResult = await createValResponse.ReadApiResponseAsync();
        createValResult.IsSuccess.Should().BeTrue();
        OptionValueListItemResponse? created = createValResult.DeserializeValue<OptionValueListItemResponse>();
        created.Should().NotBeNull();

        var updateRequest = new
        {
            name = "Rough",
            presentation = "Rough"
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/catalog/option-types/{optionType.Id}/values/{created!.Id}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        OptionValueListItemResponse? value = result.DeserializeValue<OptionValueListItemResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Rough");
        value.Presentation.Should().Be("Rough");
    }

    [Fact]
    public async Task UpdateOptionValue_WithNonexistentId_Returns404()
    {
        Guid optionTypeId = Guid.NewGuid();
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            name = "Ghost",
            presentation = "Ghost"
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/catalog/option-types/{optionTypeId}/values/{nonexistentId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
