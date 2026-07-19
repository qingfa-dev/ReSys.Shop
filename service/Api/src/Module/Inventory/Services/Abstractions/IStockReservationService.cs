using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Services.Abstractions;

/// <summary>Manages the full stock reservation lifecycle — reserve, release, expire, fulfill.</summary>
public interface IStockReservationService
{
    /// <summary>Reserves stock for an order, deducting from available inventory for a configurable TTL.</summary>
    Task<Result<StockReservation>> ReserveAsync(Guid variantId, int quantity, Guid stockLocationId, Guid orderId, int ttlMinutes = 30, CancellationToken cancellationToken = default);
    /// <summary>Releases all active reservations for an order, restoring stock quantities.</summary>
    Task ReleaseReservationsAsync(Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>Marks expired reservations and restores their stock quantities.</summary>
    Task ExpireReservationsAsync(CancellationToken cancellationToken = default);
    /// <summary>Transitions a specific reservation to fulfilled state.</summary>
    Task<Result> FulfillReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
    /// <summary>Atomically expires all overdue reservations and restores stock. Returns count processed.</summary>
    Task<int> ExpireReservationsAndRestoreStockAsync(CancellationToken cancellationToken = default);
}