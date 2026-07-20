using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Services.Abstractions;

/// <summary>Manages cart-scoped stock reservations with shorter TTL and cart-token-based release.</summary>
public interface ICartReservationService
{
    /// <summary>Reserves stock for a shopping cart session with a configurable short TTL.</summary>
    Task<Result<StockReservation>> ReserveForCartAsync(Guid variantId, int quantity, Guid stockLocationId, string cartToken, int ttlMinutes = 15, CancellationToken cancellationToken = default);
    /// <summary>Releases all active reservations for a given cart token, restoring stock quantities.</summary>
    Task ReleaseCartReservationsAsync(string cartToken, CancellationToken cancellationToken = default);
    /// <summary>Returns active reservations for a cart with remaining TTL in seconds.</summary>
    Task<List<(StockReservation Reservation, int RemainingSeconds)>> GetReservationsForCartAsync(string cartToken, CancellationToken cancellationToken = default);
}