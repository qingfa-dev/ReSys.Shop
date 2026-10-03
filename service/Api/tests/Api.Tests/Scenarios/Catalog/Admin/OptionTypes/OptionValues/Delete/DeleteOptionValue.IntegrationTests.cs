using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.OptionTypes.OptionValues.Delete;

public sealed class DeleteOptionValueIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteOptionValue_WithExistingId_ReturnsSuccess()
    {
        var createOptionTypeRequest = new
        {
            name = "Deletable Value Type",
            presentation = "Deletable",
            position = 1,
            filterable = false
        };

        HttpResponseMessage createOtResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-types", createOptionTypeRequest);
        ApiResponse createOtResult = await createOtResponse.ReadApiResponseAsync();
        createOtResult.IsSuccess.Should().BeTrue();
        OptionTypeDetailResponse? optionType = createOtResult.DeserializeValue<OptionTypeDetailResponse>();
        optionType.Should().NotBeNull();

        var createValueRequest = new
        {
            name = "Deletable Value",
            presentation = "Deletable",
            optionTypeId = optionType!.Id
        };

        HttpResponseMessage createValResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-values", createValueRequest);
        ApiResponse createValResult = await createValResponse.ReadApiResponseAsync();
        createValResult.IsSuccess.Should().BeTrue();
        OptionValueListItemResponse? created = createValResult.DeserializeValue<OptionValueListItemResponse>();
        created.Should().NotBeNull();
        Guid newId = created!.Id;

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/admin/catalog/option-values/{newId}");

        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteOptionValue_WithNonexistentId_Returns404()
    {
        Guid optionTypeId = Guid.NewGuid();
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/admin/catalog/option-values/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
