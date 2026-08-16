using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Delete;

public sealed class DeleteVariantIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteVariant_WithExistingId_ReturnsSuccess()
    {
        var createProductRequest = new
        {
            name = "Variant Delete Test",
            slug = "variant-delete-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        var addVariantRequest = new
        {
            productId = product!.Id,
            sku = "DELETE-SKU",
            isMaster = false,
            price = 9.99m
        };

        HttpResponseMessage addResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/variants", addVariantRequest);
        ApiResponse addResult = await addResponse.ReadApiResponseAsync();
        addResult.IsSuccess.Should().BeTrue();
        var created = addResult.DeserializeValue<VariantDetailResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/admin/catalog/variants/{created!.Id}");

        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    private record ProductResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task DeleteVariant_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/admin/catalog/variants/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteVariant_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.DeleteAsync(
            "/api/admin/catalog/variants/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
