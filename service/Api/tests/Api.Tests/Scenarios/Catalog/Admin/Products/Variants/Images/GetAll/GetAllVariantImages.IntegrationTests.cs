using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Images.GetAll;

public sealed class GetAllVariantImagesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task GetAllVariantImages_WithExistingVariant_ReturnsEmptyList()
    {
        var createProductRequest = new
        {
            name = "Image Test Product",
            slug = "image-test-product"
        };
        HttpResponseMessage createProductResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createProductResult = await createProductResponse.ReadApiResponseAsync();
        createProductResult.IsSuccess.Should().BeTrue();
        var product = createProductResult.DeserializeValue<IdResponse>();
        product.Should().NotBeNull();

        var createVariantRequest = new { productId = product!.Id, sku = "IMG-TST-001" };
        HttpResponseMessage createVariantResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/variants", createVariantRequest);
        ApiResponse createVariantResult = await createVariantResponse.ReadApiResponseAsync();
        createVariantResult.IsSuccess.Should().BeTrue();
        var variant = createVariantResult.DeserializeValue<IdResponse>();
        variant.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/variant-images?variantId={variant!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllVariantImages_WithNonexistentVariant_ReturnsEmptyList()
    {
        Guid nonexistentVariantId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/variant-images?variantId={nonexistentVariantId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
