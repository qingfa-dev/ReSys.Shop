using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

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
            "/api/catalog/taxonomies", taxonomyRequest);
        ApiResponse taxonomyResult = await taxonomyResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? taxonomy = taxonomyResult.DeserializeValue<TaxonomyDetailResponse>();
        taxonomy.Should().NotBeNull();

        var createRequest = new
        {
            name = "Test Nike",
            slug = "test-nike",
            presentation = "Test Nike"
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy!.Id}/taxons", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonDetailResponse? created = createResult.DeserializeValue<TaxonDetailResponse>();
        created.Should().NotBeNull();

        var updateRequest = new
        {
            name = "Test Adidas",
            slug = "test-adidas",
            presentation = "Test Adidas"
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy.Id}/taxons/{created!.Id}", updateRequest);
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
            "/api/catalog/taxonomies", taxonomyRequest);
        ApiResponse taxonomyResult = await taxonomyResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? taxonomy = taxonomyResult.DeserializeValue<TaxonomyDetailResponse>();
        taxonomy.Should().NotBeNull();

        Guid nonexistentId = Guid.NewGuid();

        var updateRequest = new
        {
            name = "Ghost",
            presentation = "Ghost"
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomy!.Id}/taxons/{nonexistentId}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
