using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Classifications.Get;

public sealed class GetProductClassificationsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record ProductResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task GetProductClassifications_WithExistingProduct_ReturnsEmptyList()
    {
        var createRequest = new
        {
            name = "Classified Product",
            slug = "classified-product"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/product-classifications?productId={product!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductClassifications_WithNonexistentProduct_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/product-classifications?productId={nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
