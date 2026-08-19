using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockMovements;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Admin.StockItems.Restock;

/// <summary>Handles restocking a stock item with backorder fulfillment.</summary>
public static partial class RestockStockItem
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the restock stock item command.</summary>
        /// <param name="command">The command containing restock data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the restock outcome and backorder details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var stockItemId = command.Id;
            var quantity = command.Request.Quantity;
            var reference = command.Request.Reference;
            var reason = command.Request.Reason;

            // Validate: Reject non-positive restock quantities
            if (quantity <= 0)
                return StockItemResult.Errors.NegativeCountOnHand;

            // Load: Fetch the stock item by identifier
            var stockItem = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(si => si.Id == stockItemId, cancellationToken);

            // Check: Return not-found if stock item does not exist
            if (stockItem is null)
                return StockItemResult.Errors.NotFound(stockItemId);

            var totalFulfilled = 0;
            var fullyFulfilled = 0;
            var partiallyFulfilled = 0;
            var remaining = quantity;

            // Check: Process backorder reservations if the stock item supports backordering
            if (stockItem.Backorderable)
            {
                var backorderReservations = await dbContext.Set<StockReservation>()
                    .Where(r => r.VariantId == stockItem.VariantId
                                && r.StockLocationId == stockItem.StockLocationId
                                && r.State == ReservationState.Reserved
                                && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
                    .OrderBy(r => r.CreatedAtUtc)
                    .ToListAsync(cancellationToken);

                // Compute: Fulfill backorder reservations in FIFO order up to the restock quantity
                foreach (var reservation in backorderReservations)
                {
                    if (remaining <= 0) break;

                    var fill = Math.Min(reservation.Quantity, remaining);
                    remaining -= fill;
                    totalFulfilled += fill;

                    if (fill >= reservation.Quantity)
                    {
                        fullyFulfilled++;
                        // Update: Mark reservation as fully fulfilled
                        reservation.State = ReservationState.Fulfilled;
                    }
                    else
                    {
                        partiallyFulfilled++;
                        // Update: Reduce reservation quantity by the fulfilled amount
                        reservation.Quantity -= fill;
                    }

                    reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

                    // Create: Record backorder fulfillment movement for audit trail
                    var movementResult = StockMovementMethod.Create(
                        stockItemId: stockItem.Id,
                        quantity: fill,
                        previousCountOnHand: stockItem.CountOnHand,
                        originatorType: "Order",
                        originatorId: reservation.OrderId,
                        reason: "Backorder fulfilled from restock",
                        action: "backorder_fulfilled");

                    if (movementResult.IsSuccess)
                        dbContext.Set<StockMovement>().Add(movementResult.Value);
                }
            }

            var previousCount = stockItem.CountOnHand;
            // Update: Add remaining stock to on-hand after backorder fulfillment
            stockItem.CountOnHand += remaining;
            stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Create: Record the restock movement for audit trail
            var restockMovement = StockMovementMethod.Create(
                stockItemId: stockItem.Id,
                quantity: quantity,
                previousCountOnHand: previousCount,
                originatorType: "Restock",
                reason: reason,
                action: "restock");

            if (restockMovement.IsFailure)
                return restockMovement.Errors;

            dbContext.Set<StockMovement>().Add(restockMovement.Value);
            var movementId = restockMovement.Value.Id;

            StockItemLoggers.Restocked(logger, Quantity: quantity, Id: stockItem.Id, ActionBy: currentUser.UserName);

            stockItem.ModifiedBy = currentUser.UserName;
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                StockItemId = stockItem.Id,
                PreviousCountOnHand = previousCount,
                NewCountOnHand = stockItem.CountOnHand,
                BackordersFulfilled = fullyFulfilled,
                PartiallyFulfilled = partiallyFulfilled,
                RemainingQuantity = remaining,
                MovementId = movementId
            };
        }
    }
}