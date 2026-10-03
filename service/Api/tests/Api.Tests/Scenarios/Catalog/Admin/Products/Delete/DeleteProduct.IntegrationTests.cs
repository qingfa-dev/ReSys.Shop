using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Delete;

public sealed class DeleteProductIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteProduct_WithExistingId_ReturnsSuccess()
    {
        var createRequest = new
        {
            name = "Deletable Product",
            slug = "deletable-product"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/products", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        var created = createResult.DeserializeValue<CreateProductResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/admin/catalog/products/{created!.Id}");

        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    private record CreateProductResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task DeleteProduct_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/admin/catalog/products/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithoutAuth_Returns401()
    {
        HttpResponseMessage response = await Client.DeleteAsync(
            "/api/admin/catalog/products/00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
