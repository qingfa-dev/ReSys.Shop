using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Services;

public class StockQuantityService : IStockQuantityService
{
    private readonly IApplicationDbContext _dbContext;

    public StockQuantityService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> DecrementStockAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

        if (stockItem is null)
            return StockItemResult.Errors.VariantNotFound(variantId);

        if (stockItem.CountOnHand < quantity)
            return StockItemResult.Errors.InsufficientStock;

        var previousCount = stockItem.CountOnHand;
        stockItem.CountOnHand -= quantity;

        var movement = StockMovementMethod.Create(
            stockItemId: stockItem.Id,
            quantity: -quantity,
            previousCountOnHand: previousCount,
            originatorType: "Order",
            originatorId: orderId,
            reason: "sold");

        if (movement.IsSuccess)
            _dbContext.Set<StockMovement>().Add(movement.Value);

        return Result.Ok();
    }

    public async Task<Result> IncrementStockAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

        if (stockItem is null)
            return StockItemResult.Errors.VariantNotFound(variantId);

        var previousCount = stockItem.CountOnHand;
        stockItem.CountOnHand += quantity;

        var movement = StockMovementMethod.Create(
            stockItemId: stockItem.Id,
            quantity: quantity,
            previousCountOnHand: previousCount,
            originatorType: "Order",
            originatorId: orderId,
            reason: "returned");

        if (movement.IsSuccess)
            _dbContext.Set<StockMovement>().Add(movement.Value);

        return Result.Ok();
    }
}
