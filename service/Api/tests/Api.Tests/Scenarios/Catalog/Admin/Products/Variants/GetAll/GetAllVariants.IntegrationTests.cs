using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.GetAll;

public sealed class GetAllVariantsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAllVariants_ReturnsAtLeastMasterVariant()
    {
        var createProductRequest = new
        {
            name = "Variants List Test",
            slug = "variants-list-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/variants?productId={product!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        var value = result.DeserializeValue<VariantsListResponse>();
        value.Should().NotBeNull();
        value!.Items.Should().NotBeEmpty();
        value.Items.Should().Contain(v => v.IsMaster);
    }

    private record ProductResponse
    {
        public Guid Id { get; init; }
    }

    private record VariantsListResponse
    {
        public List<VariantDetailResponse> Items { get; init; } = [];
    }
}
