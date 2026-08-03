using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogOptionValueSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 105;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<OptionValue>(cancellationToken))
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoOptionValueJson>("004_demo_option_values.json");
        if (json is null)
            return Result.Ok();

        foreach (var v in json)
        {
            var result = OptionValueMethod.Create(
                optionTypeId: Guid.Parse(v.OptionTypeId),
                name: v.Name, presentation: v.Presentation, position: v.Position);
            Context.Set<OptionValue>().Add(result.Value);
        }
        await SaveChangesWithIdempotencyAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoOptionValueJson
    {
        public string Id { get; init; } = default!;
        public string OptionTypeId { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Presentation { get; init; } = default!;
        public int Position { get; init; }
    }
}
