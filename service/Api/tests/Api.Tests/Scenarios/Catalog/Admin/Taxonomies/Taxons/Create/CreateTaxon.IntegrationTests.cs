using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Taxons.Create;

public sealed class CreateTaxonIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private async Task<Guid> CreateTaxonomyAsync()
    {
        var taxonomyRequest = new
        {
            name = "Test Taxonomy",
            presentation = "Test Taxonomy",
            position = 1
        };

        HttpResponseMessage taxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", taxonomyRequest);
        ApiResponse taxonomyResult = await taxonomyResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? taxonomy = taxonomyResult.DeserializeValue<TaxonomyDetailResponse>();
        return taxonomy!.Id;
    }

    [Fact]
    public async Task CreateTaxon_WithValidRequest_ReturnsCreated()
    {
        Guid taxonomyId = await CreateTaxonomyAsync();

        var request = new
        {
            name = "Test Nike",
            slug = "test-nike",
            presentation = "Test Nike"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomyId}/taxons", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        TaxonDetailResponse? value = result.DeserializeValue<TaxonDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("test nike");
    }

    [Fact]
    public async Task CreateTaxon_WithParent_ReturnsCreated()
    {
        Guid taxonomyId = await CreateTaxonomyAsync();

        var rootRequest = new
        {
            name = "Test Nike",
            slug = "test-nike",
            presentation = "Test Nike"
        };

        HttpResponseMessage rootResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomyId}/taxons", rootRequest);
        ApiResponse rootResult = await rootResponse.ReadApiResponseAsync();
        TaxonDetailResponse? root = rootResult.DeserializeValue<TaxonDetailResponse>();
        root.Should().NotBeNull();

        var childRequest = new
        {
            name = "Running",
            slug = "running",
            presentation = "Running",
            parentId = root!.Id
        };

        HttpResponseMessage childResponse = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomyId}/taxons", childRequest);
        ApiResponse childResult = await childResponse.ReadApiResponseAsync();

        childResult.IsSuccess.Should().BeTrue();
        childResult.StatusCode.Should().Be(HttpStatusCode.Created);
        TaxonDetailResponse? childValue = childResult.DeserializeValue<TaxonDetailResponse>();
        childValue.Should().NotBeNull();
        childValue!.Name.Should().Be("running");
    }

    [Fact]
    public async Task CreateTaxon_WithMissingName_Returns422()
    {
        Guid taxonomyId = await CreateTaxonomyAsync();

        var request = new
        {
            slug = "no-name",
            presentation = "No Name"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            $"/api/catalog/taxonomies/{taxonomyId}/taxons", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateTaxon_WithoutAuth_Returns401()
    {
        Guid taxonomyId = await CreateTaxonomyAsync();

        var request = new
        {
            name = "Unauthorized Taxon",
            slug = "unauthorized-taxon",
            presentation = "Unauthorized"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/catalog/taxonomies/{taxonomyId}/taxons", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
