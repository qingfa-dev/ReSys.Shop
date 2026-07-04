using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Products.Variants.OptionValues.Get;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.OptionValues.Get;

public sealed class GetVariantOptionValuesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetVariantOptionValues_WithAssignedValues_ShowsIsAssignedTrue()
    {
        var createProductRequest = new
        {
            name = "Option Values Get Test",
            slug = "option-values-get-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        var createOptionTypeRequest = new
        {
            name = "Material",
            presentation = "Material",
            position = 1,
            filterable = true
        };

        HttpResponseMessage optionTypeResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/option-types", createOptionTypeRequest);
        ApiResponse optionTypeResult = await optionTypeResponse.ReadApiResponseAsync();
        optionTypeResult.IsSuccess.Should().BeTrue();
        var optionType = optionTypeResult.DeserializeValue<OptionTypeResponse>();
        optionType.Should().NotBeNull();

        var createOptionValueRequest = new
        {
            name = "Cotton",
            presentation = "Cotton",
            position = 1
        };

        HttpResponseMessage optionValueResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/option-types/{optionType!.Id}/values", createOptionValueRequest);
        ApiResponse optionValueResult = await optionValueResponse.ReadApiResponseAsync();
        optionValueResult.IsSuccess.Should().BeTrue();
        var optionValue = optionValueResult.DeserializeValue<OptionValueResponse>();
        optionValue.Should().NotBeNull();

        HttpResponseMessage listResponse = await Client.GetAsAdminRawAsync(
            $"/api/catalog/products/{product!.Id}/variants");
        ApiResponse listResult = await listResponse.ReadApiResponseAsync();
        listResult.IsSuccess.Should().BeTrue();
        var listValue = listResult.DeserializeValue<VariantsListResponse>();
        listValue.Should().NotBeNull();
        VariantDetailResponse? variant = listValue!.Items.FirstOrDefault(v => v.IsMaster);
        variant.Should().NotBeNull();

        var assignRequest = new
        {
            optionValueIds = new[] { optionValue!.Id }
        };

        HttpResponseMessage assignResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/variants/{variant!.Id}/option-values/assign", assignRequest);
        assignResponse.IsSuccessStatusCode.Should().BeTrue();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/variants/{variant.Id}/option-values");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        GetVariantOptionValues.Response? value = result.DeserializeValue<GetVariantOptionValues.Response>();
        value.Should().NotBeNull();
        value!.Items.Should().NotBeEmpty();
        value.Items.Should().Contain(i => i.Name == "Cotton" && i.IsAssigned);
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
}
