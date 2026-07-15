using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogOptionSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 100;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasOptionTypes = await HasDataAsync<OptionType>(cancellationToken);
        if (hasOptionTypes)
            return Result.Ok();

        var jsonTypes = jsonHelper.LoadIfExists<DemoOptionTypeJson>("demo_option_types.json");
        var jsonValues = jsonHelper.LoadIfExists<DemoOptionValueJson>("demo_option_values.json");

        if (jsonTypes is not null && jsonValues is not null)
        {
            await SeedFromJsonAsync(jsonTypes, jsonValues, cancellationToken);
            return Result.Ok();
        }

        await SeedHardcodedAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task SeedFromJsonAsync(
        DemoOptionTypeJson[] types, DemoOptionValueJson[] values, CancellationToken ct)
    {
        foreach (var t in types)
        {
            var result = OptionTypeMethod.Create(
                name: t.Name, presentation: t.Presentation,
                position: t.Position, filterable: t.Filterable,
                id: Guid.Parse(t.Id));
            Context.Set<OptionType>().Add(result.Value);
        }
        await Context.SaveChangesAsync(ct);

        foreach (var v in values)
        {
            var result = OptionValueExtensions.Create(
                optionTypeId: Guid.Parse(v.OptionTypeId),
                name: v.Name, presentation: v.Presentation, position: v.Position);
            Context.Set<OptionValue>().Add(result.Value);
        }
        await Context.SaveChangesAsync(ct);
    }

    private async Task SeedHardcodedAsync(CancellationToken ct)
    {
        var sizeResult = OptionTypeMethod.Create(name: "Size", presentation: "Size", position: 0, filterable: true, id: Guid.NewGuid());
        var colorResult = OptionTypeMethod.Create(name: "Color", presentation: "Color", position: 1, filterable: true, id: Guid.NewGuid());
        var size = sizeResult.Value;
        var color = colorResult.Value;
        Context.Set<OptionType>().AddRange(size, color);
        await Context.SaveChangesAsync(ct);

        var sizeValues = new (string Name, string Presentation, int Position)[]
            { ("S", "S", 0), ("M", "M", 1), ("L", "L", 2), ("XL", "XL", 3) };
        var colorValues = new (string Name, string Presentation, int Position)[]
            { ("Red", "Red", 0), ("Blue", "Blue", 1), ("Green", "Green", 2),
              ("Black", "Black", 3), ("White", "White", 4), ("Yellow", "Yellow", 5), ("Purple", "Purple", 6) };

        foreach (var (name, presentation, position) in sizeValues)
        {
            var result = OptionValueExtensions.Create(optionTypeId: size.Id, name: name, presentation: presentation, position: position);
            Context.Set<OptionValue>().Add(result.Value);
        }
        foreach (var (name, presentation, position) in colorValues)
        {
            var result = OptionValueExtensions.Create(optionTypeId: color.Id, name: name, presentation: presentation, position: position);
            Context.Set<OptionValue>().Add(result.Value);
        }
        await Context.SaveChangesAsync(ct);
    }

    private record DemoOptionTypeJson(string Id, string Name, string Presentation, int Position, bool Filterable);
    private record DemoOptionValueJson(string Id, string OptionTypeId, string Name, string Presentation, int Position);
}
