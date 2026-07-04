using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Update;

public sealed class UpdateTaxonomyIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task UpdateTaxonomy_WithValidRequest_Returns200()
    {
        var createRequest = new
        {
            name = "Original Taxonomy",
            presentation = "Original",
            position = 1
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? created = createResult.DeserializeValue<TaxonomyDetailResponse>();
        created.Should().NotBeNull();

        var updateRequest = new
        {
            name = "Updated Taxonomy",
            presentation = "Updated",
            position = 2
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/catalog/taxonomies/{created!.Id}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        TaxonomyDetailResponse? value = result.DeserializeValue<TaxonomyDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("updated taxonomy");
        value.Presentation.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateTaxonomy_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        var updateRequest = new
        {
            name = "Ghost Taxonomy",
            presentation = "Ghost",
            position = 1
        };

        HttpResponseMessage response = await Client.PutAsAdminRawAsync(
            $"/api/catalog/taxonomies/{nonexistentId}", updateRequest);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
