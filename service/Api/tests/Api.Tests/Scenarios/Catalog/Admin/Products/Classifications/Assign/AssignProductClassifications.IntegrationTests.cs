using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Admin.Products.Classifications.Assign;

public sealed class AssignProductClassificationsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public async Task AssignProductClassifications_WithValidRequest_Returns200()
    {
        var createProductRequest = new
        {
            name = "Assign Class Product",
            slug = "assign-class-product"
        };
        HttpResponseMessage createProductResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/products", createProductRequest);
        ApiResponse createProductResult = await createProductResponse.ReadApiResponseAsync();
        createProductResult.IsSuccess.Should().BeTrue();
        var product = createProductResult.DeserializeValue<IdResponse>();
        product.Should().NotBeNull();

        var createTaxonomyRequest = new
        {
            name = "Classification Taxonomy",
            presentation = "Classification"
        };
        HttpResponseMessage createTaxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", createTaxonomyRequest);
        ApiResponse createTaxonomyResult = await createTaxonomyResponse.ReadApiResponseAsync();
        createTaxonomyResult.IsSuccess.Should().BeTrue();
        var taxonomy = createTaxonomyResult.DeserializeValue<IdResponse>();
        taxonomy.Should().NotBeNull();

        var createTaxonRequest = new
        {
            name = "Test Taxon",
            slug = "test-taxon",
            taxonomyId = taxonomy!.Id
        };
        HttpResponseMessage createTaxonResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy.Id}/taxons", createTaxonRequest);
        ApiResponse createTaxonResult = await createTaxonResponse.ReadApiResponseAsync();
        createTaxonResult.IsSuccess.Should().BeTrue();
        var taxon = createTaxonResult.DeserializeValue<IdResponse>();
        taxon.Should().NotBeNull();

        var assignRequest = new
        {
            items = new[] { new { taxonId = taxon!.Id, position = 0 } }
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/products/{product!.Id}/classifications/assign", assignRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AssignProductClassifications_WithNonexistentProduct_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();
        var assignRequest = new
        {
            items = new[] { new { taxonId = Guid.NewGuid(), position = 0 } }
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/products/{nonexistentId}/classifications/assign", assignRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
