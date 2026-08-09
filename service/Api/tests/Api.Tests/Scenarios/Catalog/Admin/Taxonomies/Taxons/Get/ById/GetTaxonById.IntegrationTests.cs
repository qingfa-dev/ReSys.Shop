using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxons.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Taxons.Get.ById;

public sealed class GetTaxonByIdIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetTaxonById_WithExistingId_Returns200()
    {
        var taxonomyRequest = new
        {
            name = "TestBrands",
            presentation = "TestBrands",
            position = 1
        };

        HttpResponseMessage taxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", taxonomyRequest);
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

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxons", taxonRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonDetailResponse? created = createResult.DeserializeValue<TaxonDetailResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/taxons/{created!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        TaxonDetailResponse? value = result.DeserializeValue<TaxonDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("test nike");
    }

    [Fact]
    public async Task GetTaxonById_WithNonexistentId_Returns404()
    {
        var taxonomyRequest = new
        {
            name = "TestBrands",
            presentation = "TestBrands",
            position = 1
        };

        HttpResponseMessage taxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", taxonomyRequest);
        ApiResponse taxonomyResult = await taxonomyResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? taxonomy = taxonomyResult.DeserializeValue<TaxonomyDetailResponse>();
        taxonomy.Should().NotBeNull();

        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/admin/catalog/taxons/{nonexistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
