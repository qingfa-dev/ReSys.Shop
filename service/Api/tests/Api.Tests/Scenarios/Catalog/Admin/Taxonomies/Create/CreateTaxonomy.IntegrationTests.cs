using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Create;

public sealed class CreateTaxonomyIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateTaxonomy_WithValidRequest_ReturnsCreated()
    {
        var request = new
        {
            name = "TestBrands",
            presentation = "TestBrands",
            position = 1
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        TaxonomyDetailResponse? value = result.DeserializeValue<TaxonomyDetailResponse>();
        value.Should().NotBeNull();
        value!.Name.Should().Be("testbrands");
        value.Presentation.Should().Be("TestBrands");
    }

    [Fact]
    public async Task CreateTaxonomy_WithDuplicateName_Returns409()
    {
        var request = new
        {
            name = "TestBrands",
            presentation = "TestBrands",
            position = 1
        };

        await Client.PostAsAdminRawAsync("/api/admin/catalog/taxonomies", request);

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateTaxonomy_WithMissingName_Returns422()
    {
        var request = new
        {
            presentation = "No Name",
            position = 1
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/admin/catalog/taxonomies", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateTaxonomy_WithoutAuth_Returns401()
    {
        var request = new
        {
            name = "Unauthorized Taxonomy",
            presentation = "Unauthorized",
            position = 1
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/admin/catalog/taxonomies", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
