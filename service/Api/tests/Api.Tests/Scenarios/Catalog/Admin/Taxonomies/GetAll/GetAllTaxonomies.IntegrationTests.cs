using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.GetAll;

public sealed class GetAllTaxonomiesIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAllTaxonomies_WithMaxPageSize_ReturnsList()
    {
        var request = new
        {
            name = "TestBrands",
            presentation = "TestBrands",
            position = 1
        };

        await Client.PostAsAdminRawAsync("/api/catalog/taxonomies", request);

        HttpResponseMessage response = await Client.GetAsAdminRawAsync("/api/catalog/taxonomies?pageSize=100");
        PagedResult<TaxonomyListItemResponse> result = await response.ReadAsPagedResultAsync<TaxonomyListItemResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAllTaxonomies_SeededTaxonomyHasAutoCreatedRootTaxon()
    {
        var request = new
        {
            name = "TestCategories",
            presentation = "TestCategories",
            position = 2
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync("/api/catalog/taxonomies", request);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? created = createResult.DeserializeValue<TaxonomyDetailResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage taxonResponse = await Client.GetAsAdminRawAsync(
            "/api/catalog/taxonomies/taxons");
        PagedResult<TaxonListItemResponse> taxonResult = await taxonResponse.ReadAsPagedResultAsync<TaxonListItemResponse>();

        taxonResult.IsSuccess.Should().BeTrue();
        taxonResult.Items.Should().ContainSingle();
        taxonResult.Items.First().Name.Should().Be("testcategories");
    }
}
