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
        {
            return Result.Ok();
        }

        var us = await Context.Set<Country>().FirstOrDefaultAsync(c => c.IsoCode == "US", cancellationToken);

        var result = StockLocationMethod.Create(
            name: "Default Warehouse",
            presentation: "Default Warehouse",
            code: "DEFAULT",
            isDefault: true,
            active: true,
            propagateAllVariants: true,
            countryId: us?.Id,
            address1: "123 Commerce Blvd",
            city: "New York",
            postalCode: "10001",
            phone: "+12025550100");

        Context.Set<StockLocation>().Add(result.Value);
        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
