using Microsoft.EntityFrameworkCore;
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

        var usedSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingSlugs = await Context.Set<Taxon>()
            .Select(t => t.Slug)
            .ToListAsync(cancellationToken);
        foreach (var s in existingSlugs)
            usedSlugs.Add(s);

        foreach (var item in json)
        {
            var slug = item.Slug;
            var original = slug;
            int suffix = 2;
            while (!usedSlugs.Add(slug))
            {
                slug = $"{original}-{suffix}";
                suffix++;
            }

            Guid? parentId = string.IsNullOrEmpty(item.ParentId) ? null : Guid.Parse(item.ParentId);
            var result = TaxonMethod.Create(
                taxonomyId: Guid.Parse(item.TaxonomyId), parentId: parentId,
                name: item.Name, presentation: item.Presentation ?? item.Name,
                description: null, position: item.Position,
                slug: slug, metaTitle: null, metaDescription: null, metaKeywords: null,
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

    private record DemoTaxonJson
    {
        public string Id { get; init; } = default!;
        public string TaxonomyId { get; init; } = default!;
        public string? ParentId { get; init; }
        public string Name { get; init; } = default!;
        public string? Presentation { get; init; }
        public string Slug { get; init; } = default!;
        public int Depth { get; init; }
        public int Lft { get; init; }
        public int Rgt { get; init; }
        public int Position { get; init; }
    }
}
