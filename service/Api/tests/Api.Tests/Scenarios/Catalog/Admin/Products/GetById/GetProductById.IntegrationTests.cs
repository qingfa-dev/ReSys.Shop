using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Products.Get.ById;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.GetById;

public sealed class GetProductByIdIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetProductById_WithCreatedProduct_Returns200()
    {
        var createRequest = new
        {
            name = "Test Product",
            slug = "test-product-byid",
            description = "Product for get by id test"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateProductResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync($"/api/catalog/products/{created!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        GetProductById.Response? value = result.DeserializeValue<GetProductById.Response>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("Test Product");
        value.Id.Should().Be(created.Id);
    }

    private record CreateProductResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
    }

    [Fact]
    public async Task GetProductById_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync($"/api/catalog/products/{nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
