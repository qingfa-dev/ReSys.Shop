using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Taxons;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogTaxonSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 120;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<Taxon>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoTaxonJson>("002_demo_taxons.json");
        if (json is null)
            return Result.Ok();

        var usedSlugsByTaxonomy = new Dictionary<Guid, HashSet<string>>();
        var existingPairs = await Context.Set<Taxon>()
            .Select(t => new { t.TaxonomyId, t.Slug })
            .ToListAsync(cancellationToken);
        foreach (var pair in existingPairs)
        {
            if (!usedSlugsByTaxonomy.TryGetValue(pair.TaxonomyId, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                usedSlugsByTaxonomy[pair.TaxonomyId] = set;
            }
            set.Add(pair.Slug);
        }

        foreach (var item in json)
        {
            var taxonomyId = Guid.Parse(item.TaxonomyId);
            if (!usedSlugsByTaxonomy.TryGetValue(taxonomyId, out var usedSlugs))
            {
                usedSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                usedSlugsByTaxonomy[taxonomyId] = usedSlugs;
            }

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
                description: item.Description, position: item.Position,
                slug: slug, metaTitle: item.MetaTitle, metaDescription: item.MetaDescription,
                metaKeywords: item.MetaKeywords,
                automatic: false, rulesMatchPolicy: null, sortOrder: null, hideFromNav: false,
                imageUrl: item.ImageUrl, squareImageUrl: item.SquareImageUrl);

            var taxon = result.Value;
            taxon.Id = Guid.Parse(item.Id);
            taxon.Lft = item.Lft;
            taxon.Rgt = item.Rgt;
            taxon.Depth = item.Depth;
            taxon.Permalink = item.Permalink ?? string.Empty;
            taxon.PrettyName = item.PrettyName ?? string.Empty;
            taxon.CreatedAtUtc = DateTimeOffset.UtcNow;
            taxon.CreatedBy = "System";

            Context.Set<Taxon>().Add(taxon);
        }

        await SaveChangesWithIdempotencyAsync(cancellationToken);

        return Result.Ok();
    }

    private record DemoTaxonJson
    {
        public string Id { get; init; } = default!;
        public string TaxonomyId { get; init; } = default!;
        public string? ParentId { get; init; }
        public string Name { get; init; } = default!;
        public string? Presentation { get; init; }
        public string? Description { get; init; }
        public string Slug { get; init; } = default!;
        public string? MetaTitle { get; init; }
        public string? MetaDescription { get; init; }
        public string? MetaKeywords { get; init; }
        public string? Permalink { get; init; }
        public string? PrettyName { get; init; }
        public string? ImageUrl { get; init; }
        public string? SquareImageUrl { get; init; }
        public int Depth { get; init; }
        public int Lft { get; init; }
        public int Rgt { get; init; }
        public int Position { get; init; }
    }
}
