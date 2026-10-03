using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

namespace Api.Tests.Scenarios.Catalog.Storefront.Taxonomies.GetTree;

public sealed class GetTaxonomyTreeIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private record IdResponse
    {
        public string Id { get; init; } = "";
    }

    [Fact]
    public async Task GetTaxonomyTree_WithExistingTaxonomy_ReturnsTree()
    {
        var createRequest = new
        {
            name = "Tree Taxonomy",
            presentation = "Tree"
        };
        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        createResult.IsSuccess.Should().BeTrue();
        string taxonomyId = createResult.DeserializeValue<IdResponse>()!.Id;

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/taxonomies/{taxonomyId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTaxonomyTree_WithNonexistentTaxonomy_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsync(
            $"/api/storefront/taxonomies/{nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
