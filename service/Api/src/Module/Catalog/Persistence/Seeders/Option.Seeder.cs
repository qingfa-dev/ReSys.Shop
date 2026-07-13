using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogOptionSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 100;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasOptionTypes = await HasDataAsync<OptionType>(cancellationToken);
        if (hasOptionTypes)
        {
            return Result.Ok();
        }

        var sizeResult = OptionTypeMethod.Create(
            name: "Size",
            presentation: "Size",
            position: 0,
            filterable: true,
            id: Guid.NewGuid());

        var colorResult = OptionTypeMethod.Create(
            name: "Color",
            presentation: "Color",
            position: 1,
            filterable: true,
            id: Guid.NewGuid());

        var size = sizeResult.Value;
        var color = colorResult.Value;

        Context.Set<OptionType>().AddRange(size, color);
        await Context.SaveChangesAsync(cancellationToken);

        var sizeValues = new (string Name, string Presentation, int Position)[]
        {
            ("S", "S", 0),
            ("M", "M", 1),
            ("L", "L", 2),
            ("XL", "XL", 3)
        };

        var colorValues = new (string Name, string Presentation, int Position)[]
        {
            ("Red", "Red", 0),
            ("Blue", "Blue", 1),
            ("Green", "Green", 2),
            ("Black", "Black", 3),
            ("White", "White", 4),
            ("Yellow", "Yellow", 5),
            ("Purple", "Purple", 6)
        };

        foreach (var (name, presentation, position) in sizeValues)
        {
            var result = OptionValueExtensions.Create(
                optionTypeId: size.Id,
                name: name,
                presentation: presentation,
                position: position);
            Context.Set<OptionValue>().Add(result.Value);
        }

        foreach (var (name, presentation, position) in colorValues)
        {
            var result = OptionValueExtensions.Create(
                optionTypeId: color.Id,
                name: name,
                presentation: presentation,
                position: position);
            Context.Set<OptionValue>().Add(result.Value);
        }

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}