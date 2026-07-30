using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Taxons.GetAll;

public sealed class GetAllTaxonsIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAllTaxons_WithExistingTaxonomy_ReturnsList()
    {
        var taxonomyRequest = new
        {
            name = "TestBrands",
            presentation = "TestBrands",
            position = 1
        };

        HttpResponseMessage taxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", taxonomyRequest);
        ApiResponse taxonomyResult = await taxonomyResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? taxonomy = taxonomyResult.DeserializeValue<TaxonomyDetailResponse>();
        taxonomy.Should().NotBeNull();

        var taxonRequest = new
        {
            name = "Test Nike",
            slug = "test-nike",
            presentation = "Test Nike",
            taxonomyId = taxonomy!.Id
        };

        HttpResponseMessage createTaxonResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies/taxons", taxonRequest);
        ApiResponse createTaxonResult = await createTaxonResponse.ReadApiResponseAsync();
        createTaxonResult.IsSuccess.Should().BeTrue("the taxon should be created successfully");

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            "/api/catalog/taxonomies/taxons");
        PagedResult<TaxonListItemResponse> result = await response.ReadAsPagedResultAsync<TaxonListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.Items.Should().Contain(i => i.Name == "test nike");
    }

    [Fact]
    public async Task GetAllTaxons_WithoutSeed_ReturnsEmptyList()
    {
        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            "/api/catalog/taxonomies/taxons");
        PagedResult<TaxonListItemResponse> result = await response.ReadAsPagedResultAsync<TaxonListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.TotalCount.Should().Be(0);
    }
}
