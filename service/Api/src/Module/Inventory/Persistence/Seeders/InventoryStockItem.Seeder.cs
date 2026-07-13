using Module.Catalog.Domain.Products.Variants;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.Inventory.Persistence.Seeders;

public sealed class InventoryStockItemSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 140;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasStockItems = await HasDataAsync<StockItem>(cancellationToken);
        if (hasStockItems)
            return Result.Ok();

        var stockLocation = await Context.Set<StockLocation>()
            .FirstOrDefaultAsync(sl => sl.Default, cancellationToken);

        if (stockLocation is null)
            return Result.Ok();

        var variants = await Context.Set<Variant>()
            .Where(v => !v.IsDeleted)
            .ToListAsync(cancellationToken);

        if (variants.Count == 0)
            return Result.Ok();

        foreach (var variant in variants)
        {
            var stockItem = CreateStockItem(stockLocation.Id, variant);
            Context.Set<StockItem>().Add(stockItem);
        }

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private static StockItem CreateStockItem(Guid stockLocationId, Variant variant)
    {
        int countOnHand = variant.Sku switch
        {
            "TEE-CTN-001-S" => 50,
            "TEE-CTN-001-M" => 75,
            "TEE-CTN-001-L" => 40,
            "TEE-CTN-001-XL" => 25,
            "TEE-CTN-001-MSTR" => 10,

            "JNS-SLM-001-30" => 30,
            "JNS-SLM-001-32" => 45,
            "JNS-SLM-001-34" => 20,
            "JNS-SLM-001-MSTR" => 5,

            "DRS-FLR-001-S" => 15,
            "DRS-FLR-001-M" => 35,
            "DRS-FLR-001-L" => 20,
            "DRS-FLR-001-MSTR" => 3,

            "BAG-LEA-001" => 12,

            "SNK-RUN-001-8" => 30,
            "SNK-RUN-001-9" => 55,
            "SNK-RUN-001-10" => 40,
            "SNK-RUN-001-MSTR" => 8,

            _ => 0
        };

        var result = StockItemMethod.Create(
            stockLocationId: stockLocationId,
            variantId: variant.Id,
            countOnHand: countOnHand,
            backorderable: countOnHand > 0);

        return result.Value!;
    }
}