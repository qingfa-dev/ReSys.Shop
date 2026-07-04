using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Products.Variants.Add;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Variants.Add;

public sealed class AddVariantIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task AddVariant_WithValidRequest_ReturnsCreated()
    {
        var createProductRequest = new
        {
            name = "Variant Test Product",
            slug = "variant-test-product"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        var request = new
        {
            sku = "TEST-001",
            isMaster = false,
            price = 29.99m
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/products/{product!.Id}/variants", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        AddVariant.Response? value = result.DeserializeValue<AddVariant.Response>();
        value.Should().NotBeNull();
        value!.Sku.Should().Be("TEST-001");
    }

    private record ProductResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task AddVariant_WithMissingSku_Returns422()
    {
        var createProductRequest = new
        {
            name = "Variant Sku Test",
            slug = "variant-sku-test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        var request = new
        {
            isMaster = false,
            price = 19.99m
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/products/{product!.Id}/variants", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddVariant_WithoutAuth_Returns401()
    {
        var request = new
        {
            sku = "UNAUTH-SKU",
            isMaster = false
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/catalog/products/00000000-0000-0000-0000-000000000000/variants", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
