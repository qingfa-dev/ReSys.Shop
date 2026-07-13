using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;

namespace Module.Inventory.Persistence.Seeders;

public sealed class InventoryStockMovementSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 150;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasMovements = await HasDataAsync<StockMovement>(cancellationToken);
        if (hasMovements)
            return Result.Ok();

        var stockItems = await Context.Set<StockItem>()
            .Where(si => si.CountOnHand > 0)
            .ToListAsync(cancellationToken);

        if (stockItems.Count == 0)
            return Result.Ok();

        foreach (var stockItem in stockItems)
        {
            var movementResult = StockMovementMethod.Create(
                stockItemId: stockItem.Id,
                quantity: stockItem.CountOnHand,
                previousCountOnHand: 0,
                originatorType: "Adjustment",
                reason: "Initial stock seeding");

            if (movementResult.IsSuccess)
                Context.Set<StockMovement>().Add(movementResult.Value);
        }

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}