using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Shared.Models;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Shared.Models;

using Shared.Operational.Persistence.Data;

namespace Api.Tests.Scenarios.Catalog.Admin.Taxonomies.Taxons.GetTree;

public sealed class GetTaxonTreeIntegrationTests(ApiFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetTaxonTree_WithNestedTaxons_ReturnsTree()
    {
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var taxons = await dbContext.Set<Taxon>().ToListAsync();
            dbContext.Set<Taxon>().RemoveRange(taxons);
            var taxonomies = await dbContext.Set<Taxonomy>().ToListAsync();
            dbContext.Set<Taxonomy>().RemoveRange(taxonomies);
            await dbContext.SaveChangesAsync();
        }

        var taxonomyRequest = new
        {
            name = "TestCategories",
            presentation = "TestCategories",
            position = 1
        };

        HttpResponseMessage taxonomyResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies", taxonomyRequest);
        ApiResponse taxonomyResult = await taxonomyResponse.ReadApiResponseAsync();
        TaxonomyDetailResponse? taxonomy = taxonomyResult.DeserializeValue<TaxonomyDetailResponse>();
        taxonomy.Should().NotBeNull();

        var rootRequest = new
        {
            name = "Electronics",
            slug = "electronics",
            presentation = "Electronics",
            taxonomyId = taxonomy!.Id
        };

        HttpResponseMessage rootResponse = await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies/taxons", rootRequest);
        ApiResponse rootResult = await rootResponse.ReadApiResponseAsync();
        TaxonDetailResponse? root = rootResult.DeserializeValue<TaxonDetailResponse>();
        root.Should().NotBeNull();

        var childRequest = new
        {
            name = "Laptops",
            slug = "laptops",
            presentation = "Laptops",
            parentId = root!.Id,
            taxonomyId = taxonomy!.Id
        };

        await Client.PostAsAdminRawAsync(
            "/api/catalog/taxonomies/taxons", childRequest);

        HttpResponseMessage response = await Client.GetAsAdminRawAsync(
            "/api/catalog/taxonomies/taxons/tree");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        TaxonTreeResponse? tree = result.DeserializeValue<TaxonTreeResponse>();
        tree.Should().NotBeNull();
        tree!.Tree.Should().NotBeEmpty();
        var names = tree.Tree.Select(i => i.Name).ToList();
        tree.Tree.Should().Contain(i => i.Name == "Electronics",
            $"actual names: [{string.Join(", ", names)}]");
    }
}
