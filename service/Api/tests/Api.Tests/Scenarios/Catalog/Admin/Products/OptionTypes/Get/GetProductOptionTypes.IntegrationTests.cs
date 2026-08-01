using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.OptionTypes.Get;

public sealed class GetProductOptionTypesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record ProductResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task GetProductOptionTypes_WithExistingProduct_ReturnsEmptyList()
    {
        var createRequest = new
        {
            name = "Option Type Product",
            slug = "option-type-product"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.DeserializeValue<ProductResponse>();
        product.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/product-option-types?productId={product!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductOptionTypes_WithNonexistentProduct_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/product-option-types?productId={nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
