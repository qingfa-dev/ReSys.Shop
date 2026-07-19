using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Services;

/// <summary>Manages stock reservation lifecycle — reserve, release, expire, and fulfill reservations with automatic stock restoration.</summary>
// Contract: pre=ReserveAsync quantity>0 && stockLocationId!=Guid.Empty, post=Result<StockReservation>
public class StockReservationService(IApplicationDbContext dbContext) : IStockReservationService
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// Reserves stock for an order, deducting from available inventory for a configurable TTL.
    /// </summary>
    /// <param name="variantId">The product variant identifier.</param>
    /// <param name="quantity">The quantity to reserve. Must be positive.</param>
    /// <param name="stockLocationId">The stock location identifier.</param>
    /// <param name="orderId">The order identifier the reservation is for.</param>
    /// <param name="ttlMinutes">Time-to-live in minutes before the reservation expires (default 30).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the StockReservation, or an error if insufficient stock.</returns>
    public async Task<Result<StockReservation>> ReserveAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        Guid orderId,
        int ttlMinutes = 30,
        CancellationToken cancellationToken = default)
    {
        // Validate: Quantity must be positive for a valid reservation
        if (quantity <= 0)
            return StockReservationResult.Errors.QuantityZero;

        // Load: Find the stock item for this variant at the specified location
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

        if (stockItem is null)
            return StockReservationResult.Errors.InsufficientStock;

        // Load: Sum already-reserved quantities for this variant and location
        var reserved = await _dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.StockLocationId == stockLocationId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .SumAsync(r => r.Quantity, cancellationToken);

        // Compute: Available stock = on-hand minus already-reserved
        var available = stockItem.CountOnHand - reserved;
        if (available < quantity)
            return StockReservationResult.Errors.InsufficientStock;

        // Create: Build the reservation entity via domain factory method
        var result = StockReservationMethod.Reserve(variantId, quantity, stockLocationId, orderId, ttlMinutes);
        if (result.IsFailure) return result;

        var reservation = result.Value;
        _dbContext.Set<StockReservation>().Add(reservation);

        return reservation;
    }

    /// <summary>Releases all active reservations for an order, restoring stock quantities.</summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ReleaseReservationsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // Load: Find all active reservations for the order
        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.OrderId == orderId && r.State == ReservationState.Reserved)
            .ToListAsync(cancellationToken);

        foreach (var r in reservations)
        {
            // Update: Release the reservation
            r.State = ReservationState.Released;
            r.ModifiedAtUtc = DateTimeOffset.UtcNow;

            if (r.StockLocationId is not null)
            {
                // Load: Find the stock item to restore quantity
                var stockItem = await _dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == r.VariantId && si.StockLocationId == r.StockLocationId.Value, cancellationToken);
                if (stockItem is not null)
                    // Update: Restore the released quantity to available stock
                    stockItem.CountOnHand += r.Quantity;
            }
        }

        if (reservations.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Marks expired reservations and restores their stock quantities.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExpireReservationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        // Load: Find all reservations that have passed their expiration time
        var expired = await _dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var r in expired)
        {
            // Update: Mark as expired
            r.State = ReservationState.Expired;
            r.ModifiedAtUtc = now;

            if (r.StockLocationId is not null)
            {
                // Load: Find the stock item to restore quantity
                var stockItem = await _dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == r.VariantId && si.StockLocationId == r.StockLocationId.Value, cancellationToken);
                if (stockItem is not null)
                    // Update: Restore expired reservation quantity back to available stock
                    stockItem.CountOnHand += r.Quantity;
            }
        }

        if (expired.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Transitions a specific reservation to fulfilled state.</summary>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success result or an error if the reservation was not found or already expired.</returns>
    public async Task<Result> FulfillReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        // Load: Find the reservation by identifier
        var reservation = await _dbContext.Set<StockReservation>()
            .FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);

        if (reservation is null)
            return StockReservationResult.Errors.NotFound(reservationId);

        // Validate: Ensure the reservation is still active before fulfilling
        if (reservation.State != ReservationState.Reserved)
            return StockReservationResult.Errors.AlreadyExpired;

        // Update: Transition to fulfilled state
        reservation.State = ReservationState.Fulfilled;
        reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    /// <summary>Atomically expires all overdue reservations and restores their stock quantities in a single transaction.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of expired reservations processed.</returns>
    public async Task<int> ExpireReservationsAndRestoreStockAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        // Load: Find all expired reservations that need cleanup
        var expired = await _dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var r in expired)
        {
            // Update: Mark as expired
            r.State = ReservationState.Expired;
            r.ModifiedAtUtc = now;

            if (r.StockLocationId is not null)
            {
                // Load: Find the stock item to restore quantity
                var stockItem = await _dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == r.VariantId && si.StockLocationId == r.StockLocationId.Value, cancellationToken);
                if (stockItem is not null)
                    // Update: Restore expired reservation quantity back to available stock
                    stockItem.CountOnHand += r.Quantity;
            }
        }

        if (expired.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        return expired.Count;
    }
}