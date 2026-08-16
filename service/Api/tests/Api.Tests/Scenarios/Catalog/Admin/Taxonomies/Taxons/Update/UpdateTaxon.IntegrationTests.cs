using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Taxons.Update;

public sealed class UpdateTaxonIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateTaxon_WithValidRequest_Returns200()
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

        var createRequest = new
        {
            name = "Test Nike",
            slug = "test-nike",
            presentation = "Test Nike",
            taxonomyId = taxonomy!.Id
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxons", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonDetailResponse? created = createResult.DeserializeValue<TaxonDetailResponse>();
        created.Should().NotBeNull();

        var updateRequest = new
        {
            name = "Test Adidas",
            slug = "test-adidas",
            presentation = "Test Adidas",
            taxonomyId = taxonomy!.Id
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/admin/catalog/taxons/{created!.Id}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        TaxonDetailResponse? value = result.DeserializeValue<TaxonDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("test adidas");
    }

    [Fact]
    public async Task UpdateTaxon_WithNonexistentId_Returns422()
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

        var updateRequest = new
        {
            name = "Ghost",
            slug = "ghost",
            presentation = "Ghost",
            taxonomyId = taxonomy!.Id
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/admin/catalog/taxons/{nonexistentId}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
