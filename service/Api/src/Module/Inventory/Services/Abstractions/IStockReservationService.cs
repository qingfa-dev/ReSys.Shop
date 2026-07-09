using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Services.Abstractions;

public interface IStockReservationService
{
    Task<Result<StockReservation>> ReserveAsync(Guid variantId, int quantity, Guid stockLocationId, Guid orderId, int ttlMinutes = 30, CancellationToken cancellationToken = default);
    Task ReleaseReservationsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task ExpireReservationsAsync(CancellationToken cancellationToken = default);
    Task<Result> FulfillReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<int> ExpireReservationsAndRestoreStockAsync(CancellationToken cancellationToken = default);
}
