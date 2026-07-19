using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockReservations;

using Shared.Application.Contracts.Inventory;

namespace Module.Inventory.Services;

/// <summary>Adjusts stock quantities — decrements for order fulfilment and increments for returns.</summary>
public class StockQuantityService : IStockQuantityService
{
    private readonly IApplicationDbContext _dbContext;

    public StockQuantityService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Decrements stock for an order, records a movement, and fulfills the matching reservation.
    /// </summary>
    /// <param name="variantId">The product variant identifier.</param>
    /// <param name="quantity">The quantity to decrement. Must be positive.</param>
    /// <param name="stockLocationId">The stock location identifier.</param>
    /// <param name="orderId">The order identifier for audit and reservation matching.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success result or an error if insufficient stock or variant not found.</returns>
    public async Task<Result> DecrementStockAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // Validate: Quantity must be positive for a valid decrement
        if (quantity <= 0)
            return StockItemResult.Errors.NegativeCountOnHand;

        // Load: Find the stock item for this variant at the specified location
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

        if (stockItem is null)
            return StockItemResult.Errors.VariantNotFound(variantId);

        // Validate: Ensure sufficient stock before decrementing
        if (stockItem.CountOnHand < quantity)
            return StockItemResult.Errors.InsufficientStock;

        var previousCount = stockItem.CountOnHand;
        // Update: Decrease the on-hand quantity
        stockItem.CountOnHand -= quantity;

        // Create: Record the outgoing stock movement for audit trail
        var movement = StockMovementMethod.Create(
            stockItemId: stockItem.Id,
            quantity: -quantity,
            previousCountOnHand: previousCount,
            originatorType: "Order",
            originatorId: orderId,
            reason: "sold");

        if (movement.IsSuccess)
            _dbContext.Set<StockMovement>().Add(movement.Value);

        // Load: Find the matching order reservation to fulfill
        var reservation = await _dbContext.Set<StockReservation>()
            .FirstOrDefaultAsync(r => r.OrderId == orderId && r.VariantId == variantId
                && r.State == ReservationState.Reserved, cancellationToken);

        if (reservation is not null)
        {
            // Update: Fulfill the reservation since stock has been decremented
            reservation.State = ReservationState.Fulfilled;
            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        return Result.Ok();
    }

    /// <summary>
    /// Increments stock for a returned order, records a movement.
    /// </summary>
    /// <param name="variantId">The product variant identifier.</param>
    /// <param name="quantity">The quantity to increment. Must be positive.</param>
    /// <param name="stockLocationId">The stock location identifier.</param>
    /// <param name="orderId">The order identifier for audit trail.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success result or an error if variant not found.</returns>
    public async Task<Result> IncrementStockAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // Validate: Quantity must be positive for a valid increment
        if (quantity <= 0)
            return StockItemResult.Errors.NegativeCountOnHand;
        // Load: Find the stock item for this variant at the specified location
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

        if (stockItem is null)
            return StockItemResult.Errors.VariantNotFound(variantId);

        var previousCount = stockItem.CountOnHand;
        // Update: Increase the on-hand quantity
        stockItem.CountOnHand += quantity;

        // Create: Record the incoming stock movement for audit trail
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