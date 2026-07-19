using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Services;

/// <summary>Manages cart-scoped stock reservations with shorter TTL (15 min) and cart-token-based release.</summary>
public class CartReservationService(IApplicationDbContext dbContext) : ICartReservationService
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    /// <summary>Reserves stock for a shopping cart session with a configurable short TTL.</summary>
    /// <param name="variantId">The product variant identifier.</param>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <param name="stockLocationId">The stock location identifier.</param>
    /// <param name="cartToken">The cart session token.</param>
    /// <param name="ttlMinutes">Time-to-live in minutes (default 15 for cart reservations).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the StockReservation, or an error if insufficient stock.</returns>
    public async Task<Result<StockReservation>> ReserveForCartAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        string cartToken,
        int ttlMinutes = 15,
        CancellationToken cancellationToken = default)
    {
        // Validate: Quantity must be positive for a valid reservation
        if (quantity <= 0)
            return StockReservationResult.Errors.QuantityZero;

        // Load: Find the stock item for this variant at the specified location
        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

        if (stockItem is null) return StockReservationResult.Errors.InsufficientStock;

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

        // Create: Build the cart reservation via domain factory method
        var result = StockReservationMethod.Reserve(variantId, quantity, stockLocationId, null, ttlMinutes, cartToken: cartToken);
        if (result.IsFailure) return result;

        var reservation = result.Value;
        _dbContext.Set<StockReservation>().Add(reservation);

        return reservation;
    }

    /// <summary>Releases all active reservations for a given cart token, restoring stock quantities.</summary>
    /// <param name="cartToken">The cart session token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ReleaseCartReservationsAsync(string cartToken, CancellationToken cancellationToken = default)
    {
        // Load: Find all active reservations for this cart token
        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == cartToken && r.State == ReservationState.Reserved)
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
    }

    /// <summary>Returns all active, non-expired reservations for a cart with their remaining TTL in seconds.</summary>
    /// <param name="cartToken">The cart session token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of (reservation, remaining seconds) tuples.</returns>
    public async Task<List<(StockReservation Reservation, int RemainingSeconds)>> GetReservationsForCartAsync(
        string cartToken, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        // Load: Find all active, non-expired reservations for this cart
        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == cartToken && r.State == ReservationState.Reserved && r.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);

        // Compute: Remaining TTL for each reservation
        return reservations
            .Select(r => (r, (int)(r.ExpiresAtUtc!.Value - now).TotalSeconds))
            .ToList();
    }
}