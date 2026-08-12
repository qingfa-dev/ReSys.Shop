using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Services.StockReservations;

public interface IStockReservationService
{
    Task<Result<StockReservation>> ReserveAsync(Guid variantId, int quantity, Guid stockLocationId, Guid? orderId = null, string? cartToken = null, int ttlMinutes = 30, CancellationToken ct = default);
    Task<Result<StockReservation>> ReserveForVariantAsync(Guid variantId, int quantity, string? cartToken = null, int ttlMinutes = 30, CancellationToken ct = default);
    Task<Result<int>> ReleaseReservationsAsync(Guid? orderId = null, string? cartToken = null, CancellationToken ct = default);
    Task<Result<int>> ReleaseCartReservationsAsync(string cartToken, Guid? variantId = null, CancellationToken ct = default);
    Task<Result> ConsumeForOrderAsync(Guid orderId, CancellationToken ct = default);
    Task<Result> ReleaseReservationAsync(Guid reservationId, CancellationToken ct = default);
    Task<Result<int>> ExpireReservationsAsync(CancellationToken ct = default);
    Task<Result<List<(StockReservation Reservation, int RemainingSeconds)>>> GetReservationsForCartAsync(string cartToken, CancellationToken ct = default);
}
