using Module.Catalog.Domain.Taxonomies;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogTaxonomySeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 110;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasTaxonomies = await HasDataAsync<Taxonomy>(cancellationToken);
        if (hasTaxonomies)
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoTaxonomyJson>("001_demo_taxonomies.json");
        if (json is null)
            return Result.Ok();

        foreach (var t in json)
        {
            var result = TaxonomyMethod.Create(
                name: t.Name, presentation: t.Presentation,
                position: t.Position, id: Guid.Parse(t.Id));
            Context.Set<Taxonomy>().Add(result.Value);
        }
        await SaveChangesWithIdempotencyAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoTaxonomyJson
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Presentation { get; init; } = default!;
        public int Position { get; init; }
    }
}
