using Module.Catalog.Domain.OptionTypes;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogOptionTypeSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 100;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<OptionType>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoOptionTypeJson>("003_demo_option_types.json");
        if (json is null)
            return Result.Ok();

        foreach (var t in json)
        {
            var result = OptionTypeMethod.Create(
                name: t.Name, presentation: t.Presentation,
                position: t.Position, filterable: t.Filterable,
                id: Guid.Parse(t.Id));
            Context.Set<OptionType>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoOptionTypeJson
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Presentation { get; init; } = default!;
        public int Position { get; init; }
        public bool Filterable { get; init; }
    }
}
