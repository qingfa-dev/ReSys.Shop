using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Products.Update;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Update;

public sealed class UpdateProductIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateProduct_WithValidRequest_Returns200()
    {
        var createRequest = new
        {
            name = "Original Product",
            slug = "original-product",
            description = "Original description"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateProductResponse>();
        created.Should().NotBeNull();

        var updateRequest = new
        {
            name = "Updated Product",
            slug = "updated-product",
            description = "Updated description"
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/catalog/products/{created!.Id}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        UpdateProduct.Response? value = result.DeserializeValue<UpdateProduct.Response>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Updated Product");
        value.Slug.Should().Be("updated-product");
    }

    private record CreateProductResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task UpdateProduct_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var request = new
        {
            name = "Ghost Product",
            slug = "ghost-product"
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/catalog/products/{nonexistentId}", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
