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

        var json = DemoJsonHelper.LoadIfExists<DemoStockItemJson>("demo_stock_items.json");
        if (json is not null)
        {
            var locations = await Context.Set<StockLocation>().ToListAsync(cancellationToken);
            var variants = await Context.Set<Variant>().Where(v => !v.IsDeleted).ToListAsync(cancellationToken);
            var variantLookup = variants.ToDictionary(v => v.Id);

            foreach (var item in json)
            {
                var location = locations.FirstOrDefault(l => l.Code == item.StockLocationCode);
                if (location is null) continue;
                if (!variantLookup.TryGetValue(Guid.Parse(item.VariantId), out _)) continue;

                var result = StockItemMethod.Create(
                    stockLocationId: location.Id,
                    variantId: Guid.Parse(item.VariantId),
                    countOnHand: item.CountOnHand,
                    backorderable: item.Backorderable);
                if (result.IsSuccess)
                    Context.Set<StockItem>().Add(result.Value);
            }
            await Context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        var stockLocation = await Context.Set<StockLocation>().FirstOrDefaultAsync(sl => sl.Default, cancellationToken);
        if (stockLocation is null) return Result.Ok();
        var allVariants = await Context.Set<Variant>().Where(v => !v.IsDeleted).ToListAsync(cancellationToken);
        foreach (var variant in allVariants)
        {
            int countOnHand = variant.Sku switch
            {
                "TEE-CTN-001-S" => 50, "TEE-CTN-001-M" => 75, "TEE-CTN-001-L" => 40, "TEE-CTN-001-XL" => 25, "TEE-CTN-001-MSTR" => 10,
                "JNS-SLM-001-30" => 30, "JNS-SLM-001-32" => 45, "JNS-SLM-001-34" => 20, "JNS-SLM-001-MSTR" => 5,
                "DRS-FLR-001-S" => 15, "DRS-FLR-001-M" => 35, "DRS-FLR-001-L" => 20, "DRS-FLR-001-MSTR" => 3,
                "BAG-LEA-001" => 12,
                "SNK-RUN-001-8" => 30, "SNK-RUN-001-9" => 55, "SNK-RUN-001-10" => 40, "SNK-RUN-001-MSTR" => 8,
                _ => 0
            };
            var result = StockItemMethod.Create(stockLocation.Id, variant.Id, countOnHand > 0, countOnHand);
            Context.Set<StockItem>().Add(result.Value!);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoStockItemJson(string VariantId, string StockLocationCode, int CountOnHand, bool Backorderable);
}
