using Module.Catalog.Domain.Taxonomies.Taxons;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogTaxonSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 120;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<Taxon>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoTaxonJson>("demo_taxons.json");
        if (json is null)
            return Result.Ok();

        foreach (var item in json)
        {
            Guid? parentId = string.IsNullOrEmpty(item.ParentId) ? null : Guid.Parse(item.ParentId);
            var result = TaxonMethod.Create(
                taxonomyId: Guid.Parse(item.TaxonomyId), parentId: parentId,
                name: item.Name, presentation: item.Presentation ?? item.Name,
                description: null, position: item.Position,
                slug: item.Slug, metaTitle: null, metaDescription: null, metaKeywords: null,
                automatic: false, rulesMatchPolicy: null, sortOrder: null, hideFromNav: false,
                imageUrl: null, squareImageUrl: null);

            var taxon = result.Value;
            taxon.Id = Guid.Parse(item.Id);
            taxon.Lft = item.Lft;
            taxon.Rgt = item.Rgt;
            taxon.Depth = item.Depth;
            taxon.CreatedAtUtc = DateTimeOffset.UtcNow;
            taxon.CreatedBy = "System";

            Context.Set<Taxon>().Add(taxon);
        }

        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoTaxonJson(string Id, string TaxonomyId, string? ParentId, string Name, string? Presentation,
        string Slug, int Depth, int Lft, int Rgt, int Position);
}
