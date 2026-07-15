using Module.Inventory.Domain.StockLocations;
using Module.Location.Domain.Countries;

namespace Module.Inventory.Persistence.Seeders;

public sealed class StockLocationSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 100;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasStockLocations = await HasDataAsync<StockLocation>(cancellationToken);
        if (hasStockLocations)
            return Result.Ok();

        var json = DemoJsonHelper.LoadIfExists<DemoStockLocationJson>("demo_stock_locations.json");
        if (json is not null)
        {
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

        var us = await Context.Set<Country>().FirstOrDefaultAsync(c => c.IsoCode == "US", cancellationToken);
        var defaultResult = StockLocationMethod.Create(
            name: "Default Warehouse", presentation: "Default Warehouse", code: "DEFAULT",
            isDefault: true, active: true, propagateAllVariants: true, countryId: us?.Id,
            address1: "123 Commerce Blvd", city: "New York", postalCode: "10001", phone: "+12025550100");
        Context.Set<StockLocation>().Add(defaultResult.Value);
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoStockLocationJson(string Id, string Name, string? Presentation, string Code,
        bool IsDefault, bool Active, string? Address1, string? City, string? PostalCode, string? Phone,
        bool BackorderableDefault, bool PropagateAllVariants, int Position, string CountryIso);
}
