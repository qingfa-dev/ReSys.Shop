using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Products.Variants.Values.Get;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.OptionValues.Revoke;

public sealed class RevokeVariantOptionValuesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task RevokeVariantOptionValues_WithAssignedValues_ReturnsSuccess()
    {
        var createProductRequest = new
        {
            name = "Revoke Option Values Test",
            slug = "revoke-option-values-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        var createOptionTypeRequest = new
        {
            name = "TestColor",
            presentation = "TestColor",
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
            name = "Red",
            presentation = "Red",
            position = 1,
            optionTypeId = optionType!.Id
        };

        HttpResponseMessage optionValueResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/option-values", createOptionValueRequest);
        ApiResponse optionValueResult = await optionValueResponse.ReadApiResponseAsync();
        optionValueResult.IsSuccess.Should().BeTrue();
        var optionValue = optionValueResult.DeserializeValue<OptionValueResponse>();
        optionValue.Should().NotBeNull();

        HttpResponseMessage listResponse = await Client.GetAsAdminRawAsync(
            $"/api/catalog/variants?productId={product!.Id}");
        ApiResponse listResult = await listResponse.ReadApiResponseAsync();
        listResult.IsSuccess.Should().BeTrue();
        var listValue = listResult.DeserializeValue<VariantsListResponse>();
        listValue.Should().NotBeNull();
        VariantDetailResponse? variant = listValue!.Items.FirstOrDefault(v => v.IsMaster);
        variant.Should().NotBeNull();

        var assignRequest = new
        {
            variantId = variant!.Id,
            optionValueIds = new[] { optionValue!.Id }
        };

        HttpResponseMessage assignResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/variant-option-values/assign", assignRequest);
        assignResponse.IsSuccessStatusCode.Should().BeTrue();

        var revokeRequest = new
        {
            variantId = variant.Id,
            optionValueIds = new[] { optionValue.Id }
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/catalog/variant-option-values/revoke", revokeRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage getResponse = await Client.GetAsAdminRawAsync(
            $"/api/catalog/variant-option-values?variantId={variant.Id}");
        PagedResult<GetVariantOptionValues.Response> value = await getResponse.ReadAsPagedResultAsync<GetVariantOptionValues.Response>();
        value.IsSuccess.Should().BeTrue();
        value.Items.Should().Contain(i => i.Name == "Red" && !i.IsAssigned);
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
    public async Task RevokeVariantOptionValues_WithNonexistentVariant_Returns404()
    {
        Guid nonexistentVariantId = Guid.NewGuid();

        var request = new
        {
            variantId = nonexistentVariantId,
            optionValueIds = new[] { Guid.NewGuid() }
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/catalog/variant-option-values/revoke", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
