using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.GetById;

public sealed class GetTaxonomyByIdIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetTaxonomyById_WithExistingId_Returns200()
    {
        var createRequest = new
        {
            name = "Test Taxonomy",
            presentation = "Test",
            position = 1
        };

        HttpResponseMessage createResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", createRequest);
        ApiResponse createResult = await createResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? created = createResult.DeserializeValue<TaxonomyDetailResponse>();
        created.Should().NotBeNull();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/taxonomies/{created!.Id}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        TaxonomyDetailResponse? value = result.DeserializeValue<TaxonomyDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("test taxonomy");
    }

    [Fact]
    public async Task GetTaxonomyById_WithNonexistentId_Returns404()
    {
        Guid nonexistentId = Guid.NewGuid();

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            $"/api/catalog/taxonomies/{nonexistentId}");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
