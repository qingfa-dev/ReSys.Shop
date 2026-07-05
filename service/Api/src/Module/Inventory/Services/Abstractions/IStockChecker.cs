using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Services.Abstractions;

/// <summary>
/// Centralized service contract for stock validation, reservations, expiry, quantity adjustments,
/// cart holds, stock transfers, backorder fulfillment, and inventory summaries.
/// </summary>
// Boundary: Services → Data — all methods operate through IApplicationDbContext
public interface IStockChecker
{
    /// <summary>
    /// Checks whether sufficient available stock exists for a variant at a specific location.
    /// Available stock = CountOnHand minus active reserved quantity.
    /// </summary>
    Task<bool> IsAvailableAsync(Guid variantId, int quantity, Guid stockLocationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether sufficient stock exists across all locations for a variant.
    /// </summary>
    Task<bool> IsAvailableAnyLocationAsync(Guid variantId, int quantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserves a quantity of stock for an order at a specific location with TTL expiry.
    /// </summary>
    Task<Result<StockReservation>> ReserveAsync(Guid variantId, int quantity, Guid stockLocationId, Guid orderId, int ttlMinutes = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases all active reservations for a given order.
    /// </summary>
    Task ReleaseReservationsAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Expires all reservations past their TTL (state change only, no stock restore).
    /// </summary>
    Task ExpireReservationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrements stock quantity for a variant at a location, creating a StockMovement.
    /// </summary>
    Task<Result> DecrementStockAsync(Guid variantId, int quantity, Guid stockLocationId, Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments stock quantity for a variant at a location (e.g., on order cancellation).
    /// </summary>
    Task<Result> IncrementStockAsync(Guid variantId, int quantity, Guid stockLocationId, Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserves stock for a cart item with configurable TTL, accounting for all active reservations.
    /// </summary>
    Task<Result<StockReservation>> ReserveForCartAsync(Guid variantId, int quantity, Guid stockLocationId, string cartToken, int ttlMinutes = 15, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases all cart reservations for a given cart token and restores stock.
    /// </summary>
    Task ReleaseCartReservationsAsync(string cartToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a reservation as fulfilled (stock already deducted at checkout).
    /// </summary>
    Task<Result> FulfillReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Expires past-TTL reservations AND restores stock to the corresponding StockItems.
    /// Returns the number of expired reservations.
    /// </summary>
    Task<int> ExpireReservationsAndRestoreStockAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns active reservations for a cart token with remaining TTL seconds.
    /// </summary>
    Task<List<(StockReservation Reservation, int RemainingSeconds)>> GetReservationsForCartAsync(string cartToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restocks a stock item, fulfilling backorders first if the item is backorderable.
    /// </summary>
    Task<Result<RestockResult>> RestockAsync(Guid stockItemId, int quantity, string? reference = null, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a stock transfer (Draft → InTransit), decrementing source stock.
    /// </summary>
    Task<Result> ExecuteTransferAsync(Guid transferId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Receives items at the destination for a stock transfer (InTransit → Received).
    /// </summary>
    Task<Result> ReceiveTransferAsync(Guid transferId, List<(Guid VariantId, int Quantity)> receivedItems, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a stock transfer (Draft|InTransit → Canceled), restoring source stock if InTransit.
    /// </summary>
    Task<Result> CancelTransferAsync(Guid transferId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns consolidated per-variant stock summary across all locations.
    /// </summary>
    Task<List<VariantStockSummary>> GetStockSummaryAsync(CancellationToken cancellationToken = default);
}
