using Module.Inventory.Domain.StockLocations;
using Module.Location.Domain.Countries;

namespace Module.Inventory.Persistence.Seeders;

public sealed class StockLocationSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 100;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasStockLocations = await HasDataAsync<StockLocation>(cancellationToken);
        if (hasStockLocations)
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoStockLocationJson>("009_demo_stock_locations.json");
        if (json is null)
            return Result.Ok();

        var countries = await Context.Set<Country>().ToListAsync(cancellationToken);
        foreach (var loc in json)
        {
            var country = countries.FirstOrDefault(c => c.IsoCode == loc.CountryIso);
            var result = StockLocationMethod.Create(
                name: loc.Name, isDefault: loc.IsDefault, active: loc.Active,
                countryId: country?.Id, presentation: loc.Presentation, code: loc.Code,
                address1: loc.Address1, city: loc.City, postalCode: loc.PostalCode,
                phone: loc.Phone, backorderableDefault: loc.BackorderableDefault,
                propagateAllVariants: loc.PropagateAllVariants,
                position: loc.Position, id: Guid.Parse(loc.Id));
            Context.Set<StockLocation>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoStockLocationJson
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string? Presentation { get; init; }
        public string Code { get; init; } = default!;
        public bool IsDefault { get; init; }
        public bool Active { get; init; }
        public string? Address1 { get; init; }
        public string? City { get; init; }
        public string? PostalCode { get; init; }
        public string? Phone { get; init; }
        public bool BackorderableDefault { get; init; }
        public bool PropagateAllVariants { get; init; }
        public int Position { get; init; }
        public string CountryIso { get; init; } = default!;
    }
}
