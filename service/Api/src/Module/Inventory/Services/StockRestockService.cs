using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.Abstractions;
using Module.Inventory.Services.Models;

namespace Module.Inventory.Services;

/// <summary>Processes restock operations, fulfilling pending backorders before adding remaining stock to on-hand.</summary>
// Contract: pre=quantity>0, post=Result<RestockResult> with stockItem.CountOnHand increased
public class StockRestockService(IApplicationDbContext dbContext) : IStockRestockService
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// Restocks a stock item, first fulfilling any pending backorder reservations in FIFO order.
    /// </summary>
    /// <param name="stockItemId">The stock item identifier.</param>
    /// <param name="quantity">The incoming restock quantity. Must be positive.</param>
    /// <param name="reference">Optional reference number for the restock.</param>
    /// <param name="reason">Optional reason for the restock.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the restock summary with backorder fulfillment details.</returns>
    public async Task<Result<RestockResult>> RestockAsync(
        Guid stockItemId,
        int quantity,
        string? reference = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        // Validate: Quantity must be positive for a valid restock
        if (quantity <= 0)
            return StockItemResult.Errors.NegativeCountOnHand;

        // Load: Find the stock item by identifier
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.Id == stockItemId, cancellationToken);

        if (stockItem is null)
            return StockItemResult.Errors.NotFound(stockItemId);

        var previousCount = stockItem.CountOnHand;

        // Create: Fulfill pending backorders from the incoming restock quantity
        var backorderResult = await FulfillBackordersInternalAsync(stockItem, quantity, cancellationToken);
        var remainingAfterBackorders = quantity - backorderResult.TotalFulfilled;

        // Update: Add remaining stock to on-hand after backorder fulfillment
        stockItem.CountOnHand += remainingAfterBackorders;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        // Create: Record the restock movement for audit trail
        var movement = StockMovementMethod.Create(
            stockItemId: stockItem.Id,
            quantity: quantity,
            previousCountOnHand: previousCount,
            originatorType: "Restock",
            reason: reason,
            action: "restock");

        if (movement.IsSuccess)
        {
            _dbContext.Set<StockMovement>().Add(movement.Value);
        }

        return new RestockResult
        {
            StockItemId = stockItem.Id,
            PreviousCountOnHand = previousCount,
            NewCountOnHand = stockItem.CountOnHand,
            BackordersFulfilled = backorderResult.FullyFulfilled,
            PartiallyFulfilled = backorderResult.PartiallyFulfilled,
            RemainingQuantity = remainingAfterBackorders,
            MovementId = movement.IsSuccess ? movement.Value.Id : Guid.Empty
        };
    }

    private async Task<BackorderFulfillmentResult> FulfillBackordersInternalAsync(
        StockItem stockItem,
        int restockQuantity,
        CancellationToken cancellationToken)
    {
        var result = new BackorderFulfillmentResult();

        if (!stockItem.Backorderable)
            return result;

        // Load: Find pending backorder reservations in FIFO order
        var backorderReservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == stockItem.VariantId
                        && r.StockLocationId == stockItem.StockLocationId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var remaining = restockQuantity;
        foreach (var reservation in backorderReservations)
        {
            if (remaining <= 0) break;

            // Compute: How much of this reservation can be fulfilled
            var fill = Math.Min(reservation.Quantity, remaining);
            remaining -= fill;
            result.TotalFulfilled += fill;

            if (fill >= reservation.Quantity)
            {
                result.FullyFulfilled++;
                // Update: Mark reservation as fully fulfilled
                reservation.State = ReservationState.Fulfilled;
            }
            else
            {
                result.PartiallyFulfilled++;
                // Update: Reduce the reservation quantity by the fulfilled amount
                reservation.Quantity -= fill;
            }

            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Create: Record the backorder fulfillment movement
            var movementResult = StockMovementMethod.Create(
                stockItemId: stockItem.Id,
                quantity: fill,
                previousCountOnHand: stockItem.CountOnHand,
                originatorType: "Order",
                originatorId: reservation.OrderId,
                reason: "Backorder fulfilled from restock",
                action: "backorder_fulfilled");

            if (movementResult.IsSuccess)
                _dbContext.Set<StockMovement>().Add(movementResult.Value);
        }

        return result;
    }
}