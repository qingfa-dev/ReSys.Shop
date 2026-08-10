using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Variants.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.OptionValues.Assign;

public sealed class AssignVariantOptionValuesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AssignVariantOptionValues_WithValidValues_ReturnsSuccess()
    {
        var createProductRequest = new
        {
            name = "Assign Option Values Test",
            slug = "assign-option-values-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        var createOptionTypeRequest = new
        {
            name = "TestSize",
            presentation = "TestSize",
            position = 1,
            filterable = true
        };

        HttpResponseMessage optionTypeResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-types", createOptionTypeRequest);
        ApiResponse optionTypeResult = await optionTypeResponse.ReadApiResponseAsync();
        optionTypeResult.IsSuccess.Should().BeTrue();
        var optionType = optionTypeResult.DeserializeValue<OptionTypeResponse>();
        optionType.Should().NotBeNull();

        var createOptionValueRequest = new
        {
            name = "Large",
            presentation = "Large",
            position = 1,
            optionTypeId = optionType!.Id
        };

        HttpResponseMessage optionValueResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/option-values", createOptionValueRequest);
        ApiResponse optionValueResult = await optionValueResponse.ReadApiResponseAsync();
        optionValueResult.IsSuccess.Should().BeTrue();
        var optionValue = optionValueResult.DeserializeValue<OptionValueResponse>();
        optionValue.Should().NotBeNull();

        HttpResponseMessage listResponse = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/variants?productId={product!.Id}");
        ApiResponse listResult = await listResponse.ReadApiResponseAsync();
        listResult.IsSuccess.Should().BeTrue();
        var listValue = listResult.DeserializeValue<VariantsListResponse>();
        listValue.Should().NotBeNull();
        VariantDetailResponse? variant = listValue!.Items.FirstOrDefault(v => v.IsMaster);
        variant.Should().NotBeNull();

        var request = new
        {
            variantId = variant!.Id,
            optionValueIds = new[] { optionValue!.Id }
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/variant-option-values/assign", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private record ProductResponse
    {
        public Guid Id { get; init; }
    }

    private record VariantsListResponse
    {
        public List<VariantDetailResponse> Items { get; init; } = [];
    }

    private record OptionTypeResponse
    {
        public Guid Id { get; init; }
    }

    private record OptionValueResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task AssignVariantOptionValues_WithEmptyList_ReturnsError()
    {
        var createProductRequest = new
        {
            name = "Empty Assign Test",
            slug = "empty-assign-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        HttpResponseMessage listResponse = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/variants?productId={product!.Id}");
        ApiResponse listResult = await listResponse.ReadApiResponseAsync();
        listResult.IsSuccess.Should().BeTrue();
        var listValue = listResult.DeserializeValue<VariantsListResponse>();
        listValue.Should().NotBeNull();
        VariantDetailResponse? variant = listValue!.Items.FirstOrDefault(v => v.IsMaster);
        variant.Should().NotBeNull();

        var request = new
        {
            variantId = variant!.Id,
            optionValueIds = Array.Empty<Guid>()
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/variant-option-values/assign", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
