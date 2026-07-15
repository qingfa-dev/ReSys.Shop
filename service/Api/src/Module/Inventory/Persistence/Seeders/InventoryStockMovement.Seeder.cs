using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockLocations;

namespace Module.Inventory.Persistence.Seeders;

public sealed class InventoryStockMovementSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 150;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasMovements = await HasDataAsync<StockMovement>(cancellationToken);
        if (hasMovements)
            return Result.Ok();

        var json = DemoJsonHelper.LoadIfExists<DemoStockMovementJson>("demo_stock_movements.json");
        if (json is not null)
        {
            var stockItems = await Context.Set<StockItem>().ToListAsync(cancellationToken);
            var locations = await Context.Set<StockLocation>().ToListAsync(cancellationToken);

            foreach (var m in json)
            {
                var location = locations.FirstOrDefault(l => l.Code == m.StockLocationCode);
                if (location is null) continue;
                var stockItem = stockItems.FirstOrDefault(si =>
                    si.VariantId == Guid.Parse(m.VariantId) && si.StockLocationId == location.Id);
                if (stockItem is null) continue;

                var result = StockMovementMethod.Create(
                    stockItemId: stockItem.Id, quantity: m.Quantity,
                    previousCountOnHand: m.PreviousCountOnHand,
                    originatorType: m.OriginatorType, reason: m.Reason,
                    action: m.Action, stockLocationId: location.Id);
                if (result.IsSuccess)
                    Context.Set<StockMovement>().Add(result.Value);
            }
            await Context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        var items = await Context.Set<StockItem>().Where(si => si.CountOnHand > 0).ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            var result = StockMovementMethod.Create(item.Id, item.CountOnHand, 0, "Adjustment", reason: "Initial stock seeding", action: "restock");
            Context.Set<StockMovement>().Add(result.Value);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private record DemoStockMovementJson(string VariantId, string StockLocationCode, int Quantity,
        int PreviousCountOnHand, string OriginatorType, string Reason, string Action);
}
