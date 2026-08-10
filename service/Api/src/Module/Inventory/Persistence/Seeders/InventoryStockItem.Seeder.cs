using Module.Catalog.Domain.Variants;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.Inventory.Persistence.Seeders;

public sealed class InventoryStockItemSeeder(IApplicationDbContext context, DemoJsonHelper jsonHelper) : AbstractDataSeeder(context)
{
    public override int Order => 140;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasStockItems = await HasDataAsync<StockItem>(cancellationToken);
        if (hasStockItems)
            return Result.Ok();

        var json = jsonHelper.LoadIfExists<DemoStockItemJson>("010_demo_stock_items.json");
        if (json is null)
            return Result.Ok();

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
        await SaveChangesWithIdempotencyAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoStockItemJson
    {
        public string VariantId { get; init; } = default!;
        public string StockLocationCode { get; init; } = default!;
        public int CountOnHand { get; init; }
        public bool Backorderable { get; init; }
    }
}
