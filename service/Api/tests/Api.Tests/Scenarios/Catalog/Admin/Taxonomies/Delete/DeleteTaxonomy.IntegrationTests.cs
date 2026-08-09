using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Delete;

public sealed class DeleteTaxonomyIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeleteTaxonomy_WithExistingId_ReturnsSuccess()
    {
        var createRequest = new
        {
            name = "Deletable Taxonomy",
            presentation = "Deletable",
            position = 1
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? created = createResult.DeserializeValue<TaxonomyDetailResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/admin/catalog/taxonomies/{created!.Id}");
        ApiResponse result = await deleteResponse.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTaxonomy_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage deleteResponse = await Client.DeleteAsAdminRawAsync(
            $"/api/admin/catalog/taxonomies/{nonexistentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTaxonomy_WithoutAuth_Returns401()
    {
        var createRequest = new
        {
            name = "Auth Test Taxonomy",
            presentation = "Auth Test",
            position = 1
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? created = createResult.DeserializeValue<TaxonomyDetailResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/admin/catalog/taxonomies/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
