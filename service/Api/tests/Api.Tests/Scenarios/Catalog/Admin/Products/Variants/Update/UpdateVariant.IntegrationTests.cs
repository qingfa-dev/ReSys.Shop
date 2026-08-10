using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Variants.Shared.Models;
using Module.Catalog.Features.Admin.Variants.Update;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Update;

public sealed class UpdateVariantIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateVariant_WithValidRequest_Returns200()
    {
        var createProductRequest = new
        {
            name = "Variant Update Test",
            slug = "variant-update-test"
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

        var updateRequest = new
        {
            sku = "UPDATED-SKU"
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/admin/catalog/variants/{variant!.Id}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        UpdateVariant.Response? value = result.DeserializeValue<UpdateVariant.Response>();
        value.Should().NotBeNull();
        value!.Sku.Should().Be("UPDATED-SKU");
    }

    private record ProductResponse
    {
        public Guid Id { get; init; }
    }

    private record VariantsListResponse
    {
        public List<VariantDetailResponse> Items { get; init; } = [];
    }

    [Fact]
    public async Task UpdateVariant_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            sku = "GHOST-SKU"
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/admin/catalog/variants/{nonexistentId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
