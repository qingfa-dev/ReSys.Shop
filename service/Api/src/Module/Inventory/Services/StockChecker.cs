using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Services;

/// <summary>
/// Centralized service for stock validation, reservations, expiry, and quantity adjustments.
/// Accounts for active reservations when computing availability.
/// </summary>
// @CAT-10 Boundary: Domain → Data — queries StockItem/StockReservation/StockMovement via IApplicationDbContext
public class StockChecker : IStockChecker
{
    private readonly IApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockChecker"/> class.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public StockChecker(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Checks whether sufficient available stock exists for a variant at a specific location.
    /// Available stock = CountOnHand minus active reserved quantity.
    /// </summary>
    /// <param name="variantId">The variant identifier.</param>
    /// <param name="quantity">The requested quantity.</param>
    /// <param name="stockLocationId">The stock location identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if enough stock is available, otherwise false.</returns>
    // @CAT-3 Compute: Available = CountOnHand - reservedQuantity
    public async Task<bool> IsAvailableAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return true;

        // Query: Get the stock item for this variant at this location.
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

        if (stockItem is null) return false;

        // Compute: Subtract active reserved quantity from on-hand.
        var reserved = await _dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.StockLocationId == stockLocationId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .SumAsync(r => r.Quantity, cancellationToken);

        var available = stockItem.CountOnHand - reserved;
        return available >= quantity;
    }

    /// <summary>
    /// Checks whether sufficient stock exists across all locations for a variant.
    /// </summary>
    /// <param name="variantId">The variant identifier.</param>
    /// <param name="quantity">The requested quantity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if any location has enough available stock.</returns>
    public async Task<bool> IsAvailableAnyLocationAsync(
        Guid variantId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return true;

        var stockItems = await _dbContext.Set<StockItem>()
            .Where(si => si.VariantId == variantId)
            .ToListAsync(cancellationToken);

        foreach (var si in stockItems)
        {
            if (await IsAvailableAsync(variantId, quantity, si.StockLocationId, cancellationToken))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reserves a quantity of stock for an order at a specific location.
    /// </summary>
    /// <param name="variantId">The variant identifier.</param>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <param name="stockLocationId">The stock location identifier.</param>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="ttlMinutes">Time-to-live in minutes (default 30).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the reservation or an error.</returns>
    // @CAT-2 Create: Stock reservation with TTL expiry
    public async Task<Result<StockReservation>> ReserveAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        Guid orderId,
        int ttlMinutes = 30,
        CancellationToken cancellationToken = default)
    {
        var result = StockReservationExtensions.Reserve(variantId, quantity, stockLocationId, orderId, ttlMinutes);
        if (result.IsFailure) return result;

        var reservation = result.Value;
        _dbContext.Set<StockReservation>().Add(reservation);

        return reservation;
    }

    /// <summary>
    /// Releases all active reservations for a given order.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    // @CAT-2 Update: Release all reservations for an order
    public async Task ReleaseReservationsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.OrderId == orderId && r.State == ReservationState.Reserved)
            .ToListAsync(cancellationToken);

        foreach (var r in reservations)
        {
            r.State = ReservationState.Released;
            r.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// Expires all reservations past their TTL.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    // @CAT-4 Enforce: Mark expired reservations as Expired
    public async Task ExpireReservationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expired = await _dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var r in expired)
        {
            r.State = ReservationState.Expired;
            r.ModifiedAtUtc = now;
        }
    }

    /// <summary>
    /// Decrements stock quantity for a variant at a location.
    /// </summary>
    /// <param name="variantId">The variant identifier.</param>
    /// <param name="quantity">The quantity to decrement.</param>
    /// <param name="stockLocationId">The stock location identifier.</param>
    /// <param name="orderId">The order identifier for audit.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    // @CAT-2 Update: Decrement CountOnHand and create StockMovement
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

        var movement = StockMovementExtensions.Create(
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

    /// <summary>
    /// Increments stock quantity for a variant at a location (e.g., on order cancellation).
    /// </summary>
    /// <param name="variantId">The variant identifier.</param>
    /// <param name="quantity">The quantity to increment.</param>
    /// <param name="stockLocationId">The stock location identifier.</param>
    /// <param name="orderId">The order identifier for audit.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    // @CAT-2 Update: Increment CountOnHand and create StockMovement
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

        var movement = StockMovementExtensions.Create(
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

    // @CAT-2 Create: Reserve stock for a cart with TTL, accounting for all active reservations
    public async Task<Result<StockReservation>> ReserveForCartAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        string cartToken,
        int ttlMinutes = 15,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return StockReservationResult.Errors.QuantityZero;

        // Check: Verify availability accounting for ALL active reservations
        var isAvailable = await IsAvailableAsync(variantId, quantity, stockLocationId, cancellationToken);
        if (!isAvailable)
            return StockReservationResult.Errors.InsufficientStock;

        var result = StockReservationExtensions.Reserve(variantId, quantity, stockLocationId, null, ttlMinutes);
        if (result.IsFailure) return result;

        var reservation = result.Value;
        reservation.CartToken = cartToken;
        _dbContext.Set<StockReservation>().Add(reservation);

        return reservation;
    }

    // @CAT-2 Update: Release all cart reservations and restore stock
    public async Task ReleaseCartReservationsAsync(string cartToken, CancellationToken cancellationToken = default)
    {
        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == cartToken && r.State == ReservationState.Reserved)
            .ToListAsync(cancellationToken);

        foreach (var r in reservations)
        {
            // Restore stock if reservation was active on a specific location
            if (r.StockLocationId.HasValue)
            {
                var stockItem = await _dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == r.VariantId
                        && si.StockLocationId == r.StockLocationId, cancellationToken);

                if (stockItem is not null)
                    stockItem.CountOnHand += r.Quantity;
            }

            r.State = ReservationState.Released;
            r.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    // @CAT-2 Update: Mark reservation as fulfilled (stock already deducted at checkout)
    public async Task<Result> FulfillReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _dbContext.Set<StockReservation>()
            .FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);

        if (reservation is null)
            return StockReservationResult.Errors.NotFound(reservationId);

        if (reservation.State != ReservationState.Reserved)
            return StockReservationResult.Errors.AlreadyExpired;

        reservation.State = ReservationState.Fulfilled;
        reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    // @CAT-4 Enforce: Expire past-TTL reservations AND restore stock
    public async Task<int> ExpireReservationsAndRestoreStockAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expired = await _dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var r in expired)
        {
            // Restore stock to the reserved location
            if (r.StockLocationId.HasValue)
            {
                var stockItem = await _dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == r.VariantId
                        && si.StockLocationId == r.StockLocationId, cancellationToken);

                if (stockItem is not null)
                    stockItem.CountOnHand += r.Quantity;
            }

            r.State = ReservationState.Expired;
            r.ModifiedAtUtc = now;
        }

        if (expired.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        return expired.Count;
    }

    // @CAT-1 Query: Get active reservations for a cart with remaining TTL
    public async Task<List<(StockReservation Reservation, int RemainingSeconds)>> GetReservationsForCartAsync(
        string cartToken, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == cartToken && r.State == ReservationState.Reserved && r.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);

        return reservations
            .Select(r => (r, (int)(r.ExpiresAtUtc!.Value - now).TotalSeconds))
            .ToList();
    }

    // @CAT-2 Update: Restock a stock item and fulfill backorders
    public async Task<Result<RestockResult>> RestockAsync(
        Guid stockItemId,
        int quantity,
        string? reference = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return StockItemResult.Errors.NegativeCountOnHand;

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.Id == stockItemId, cancellationToken);

        if (stockItem is null)
            return StockItemResult.Errors.NotFound(stockItemId);

        var previousCount = stockItem.CountOnHand;

        // Fulfill backorders first if the item is backorderable
        var backorderResult = await FulfillBackordersInternalAsync(stockItem, quantity, cancellationToken);
        var remainingAfterBackorders = quantity - backorderResult.TotalFulfilled;

        // Add remaining to on-hand
        stockItem.CountOnHand += remainingAfterBackorders;
        stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

        // Create StockMovement for the restock
        var movement = StockMovementExtensions.Create(
            stockItemId: stockItem.Id,
            quantity: quantity,
            previousCountOnHand: previousCount,
            originatorType: "Restock",
            reason: reason);

        if (movement.IsSuccess)
        {
            movement.Value.Action = "restock";
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

    // Backorder fulfillment: fill oldest backorders first
    private async Task<BackorderFulfillmentResult> FulfillBackordersInternalAsync(
        StockItem stockItem,
        int restockQuantity,
        CancellationToken cancellationToken)
    {
        var result = new BackorderFulfillmentResult();

        if (!stockItem.Backorderable)
            return result;

        // Query: Find pending reservations for this variant that represent backorders
        // Backorders are reservations where stock was reserved but not yet physically available
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

            var fill = Math.Min(reservation.Quantity, remaining);
            remaining -= fill;
            result.TotalFulfilled += fill;

            if (fill >= reservation.Quantity)
            {
                result.FullyFulfilled++;
                reservation.State = ReservationState.Fulfilled;
            }
            else
            {
                result.PartiallyFulfilled++;
                reservation.Quantity -= fill;
            }

            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Create BackorderFulfilled movement
            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                StockItemId = stockItem.Id,
                Quantity = fill,
                PreviousCountOnHand = stockItem.CountOnHand,
                Action = "backorder_fulfilled",
                OriginatorType = "Order",
                OriginatorId = reservation.OrderId,
                Reason = "Backorder fulfilled from restock",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                CreatedBy = "System"
            };
            _dbContext.Set<StockMovement>().Add(movement);
        }

        return result;
    }

    // @CAT-2 Update: Execute a stock transfer (Draft → InTransit)
    public async Task<Result> ExecuteTransferAsync(Guid transferId, CancellationToken cancellationToken = default)
    {
        var transfer = await _dbContext.Set<StockTransfer>()
            .Include(t => t.TransferItems)
            .FirstOrDefaultAsync(t => t.Id == transferId, cancellationToken);

        if (transfer is null)
            return StockTransferResult.Failure.NotFound;

        // Validate: Source has enough stock for each item
        foreach (var item in transfer.TransferItems)
        {
            var stockItem = await _dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(si => si.VariantId == item.VariantId
                    && si.StockLocationId == transfer.SourceLocationId, cancellationToken);

            if (stockItem is null || stockItem.CountOnHand < item.Quantity)
                return StockTransferResult.Failure.InsufficientStockAtSource;
        }

        // Transition state
        var transitionResult = transfer.Transfer();
        if (transitionResult.IsFailure) return transitionResult;

        // Decrement source stock and create movements
        foreach (var item in transfer.TransferItems)
        {
            var stockItem = await _dbContext.Set<StockItem>()
                .FirstAsync(si => si.VariantId == item.VariantId
                    && si.StockLocationId == transfer.SourceLocationId, cancellationToken);

            var previousCount = stockItem.CountOnHand;
            stockItem.CountOnHand -= item.Quantity;
            stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                StockItemId = stockItem.Id,
                StockLocationId = transfer.SourceLocationId,
                Quantity = -item.Quantity,
                PreviousCountOnHand = previousCount,
                Action = "transfer_out",
                OriginatorType = "Transfer",
                OriginatorId = transfer.Id,
                Reason = $"Transfer {transfer.Number} to {transfer.DestinationLocationId}",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                CreatedBy = "System"
            };
            _dbContext.Set<StockMovement>().Add(movement);
        }

        return Result.Ok();
    }

    // @CAT-2 Update: Receive a stock transfer (InTransit → Received)
    public async Task<Result> ReceiveTransferAsync(
        Guid transferId,
        List<(Guid VariantId, int Quantity)> receivedItems,
        CancellationToken cancellationToken = default)
    {
        var transfer = await _dbContext.Set<StockTransfer>()
            .Include(t => t.TransferItems)
            .FirstOrDefaultAsync(t => t.Id == transferId, cancellationToken);

        if (transfer is null)
            return StockTransferResult.Failure.NotFound;

        if (transfer.State != TransferState.InTransit)
            return StockTransferResult.Failure.InvalidStateTransition(transfer.State, TransferState.Received);

        foreach (var (variantId, quantity) in receivedItems)
        {
            var receiveResult = transfer.Receive(variantId, quantity);
            if (receiveResult.IsFailure) return receiveResult;

            // Increment destination stock
            var destStockItem = await _dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(si => si.VariantId == variantId
                    && si.StockLocationId == transfer.DestinationLocationId, cancellationToken);

            if (destStockItem is not null)
            {
                var previousCount = destStockItem.CountOnHand;
                destStockItem.CountOnHand += quantity;
                destStockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;

                var movement = new StockMovement
                {
                    Id = Guid.NewGuid(),
                    StockItemId = destStockItem.Id,
                    StockLocationId = transfer.DestinationLocationId,
                    Quantity = quantity,
                    PreviousCountOnHand = previousCount,
                    Action = "transfer_in",
                    OriginatorType = "Transfer",
                    OriginatorId = transfer.Id,
                    Reason = $"Received from transfer {transfer.Number}",
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    CreatedBy = "System"
                };
                _dbContext.Set<StockMovement>().Add(movement);
            }
        }

        return Result.Ok();
    }

    // @CAT-2 Update: Cancel a stock transfer (restore source stock if InTransit)
    public async Task<Result> CancelTransferAsync(Guid transferId, CancellationToken cancellationToken = default)
    {
        var transfer = await _dbContext.Set<StockTransfer>()
            .Include(t => t.TransferItems)
            .FirstOrDefaultAsync(t => t.Id == transferId, cancellationToken);

        if (transfer is null)
            return StockTransferResult.Failure.NotFound;

        var wasInTransit = transfer.State == TransferState.InTransit;
        var cancelResult = transfer.Cancel();
        if (cancelResult.IsFailure) return cancelResult;

        // Restore source stock if was InTransit
        if (wasInTransit)
        {
            foreach (var item in transfer.TransferItems)
            {
                var stockItem = await _dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == item.VariantId
                        && si.StockLocationId == transfer.SourceLocationId, cancellationToken);

                if (stockItem is not null)
                {
                    stockItem.CountOnHand += item.Quantity;
                    stockItem.ModifiedAtUtc = DateTimeOffset.UtcNow;
                }
            }
        }

        return Result.Ok();
    }

    // @CAT-1 Query: Consolidated per-variant stock summary across all locations
    public async Task<List<VariantStockSummary>> GetStockSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var stockItems = await _dbContext.Set<StockItem>()
            .Include(si => si.StockLocation)
            .Where(si => si.StockLocation != null && !si.StockLocation.IsDeleted && si.StockLocation.Active)
            .ToListAsync(cancellationToken);

        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc > now)
            .GroupBy(r => new { r.VariantId, r.StockLocationId })
            .Select(g => new { g.Key.VariantId, g.Key.StockLocationId, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(cancellationToken);

        var reservationMap = reservations
            .Where(r => r.StockLocationId.HasValue)
            .GroupBy(r => r.VariantId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(r => r.StockLocationId!.Value, r => r.Reserved));

        var grouped = stockItems
            .GroupBy(si => si.VariantId)
            .Select(g =>
            {
                var locationReservations = reservationMap.GetValueOrDefault(g.Key) ?? [];
                var locationBreakdown = g.Select(si =>
                {
                    var reserved = locationReservations.GetValueOrDefault(si.StockLocationId, 0);
                    var available = si.CountOnHand - reserved;
                    return new LocationStockInfo
                    {
                        LocationId = si.StockLocationId,
                        LocationName = si.StockLocation?.Name ?? "Unknown",
                        CountOnHand = si.CountOnHand,
                        Reserved = reserved,
                        Available = available >= 0 ? available : 0,
                        IsLowStock = si.StockLocation != null && si.CountOnHand <= si.StockLocation.LowStockThreshold
                    };
                }).ToList();

                var totalOnHand = locationBreakdown.Sum(l => l.CountOnHand);
                var totalReserved = locationBreakdown.Sum(l => l.Reserved);
                var totalAvailable = locationBreakdown.Sum(l => l.Available);

                return new VariantStockSummary
                {
                    VariantId = g.Key,
                    TotalOnHand = totalOnHand,
                    TotalReserved = totalReserved,
                    TotalAvailable = totalAvailable >= 0 ? totalAvailable : 0,
                    LocationBreakdown = locationBreakdown
                };
            })
            .ToList();

        return grouped;
    }
}
