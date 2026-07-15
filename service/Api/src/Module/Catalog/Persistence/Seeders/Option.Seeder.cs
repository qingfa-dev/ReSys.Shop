using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogOptionSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 100;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        if (await HasDataAsync<OptionType>(cancellationToken))
            return Result.Ok();

        var jsonTypes = jsonHelper.LoadIfExists<DemoOptionTypeJson>("demo_option_types.json");
        var jsonValues = jsonHelper.LoadIfExists<DemoOptionValueJson>("demo_option_values.json");

        if (jsonTypes is null || jsonValues is null)
            return Result.Ok();

        foreach (var t in jsonTypes)
        {
            var result = OptionTypeMethod.Create(
                name: t.Name, presentation: t.Presentation,
                position: t.Position, filterable: t.Filterable,
                id: Guid.Parse(t.Id));
            Context.Set<OptionType>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);

        foreach (var v in jsonValues)
        {
            var result = OptionValueMethod.Create(
                optionTypeId: Guid.Parse(v.OptionTypeId),
                name: v.Name, presentation: v.Presentation, position: v.Position);
            Context.Set<OptionValue>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private record DemoOptionTypeJson(string Id, string Name, string Presentation, int Position, bool Filterable);
    private record DemoOptionValueJson(string Id, string OptionTypeId, string Name, string Presentation, int Position);
}
