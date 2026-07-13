using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Services.Abstractions;

public interface ICartReservationService
{
    Task<Result<StockReservation>> ReserveForCartAsync(Guid variantId, int quantity, Guid stockLocationId, string cartToken, int ttlMinutes = 15, CancellationToken cancellationToken = default);
    Task ReleaseCartReservationsAsync(string cartToken, CancellationToken cancellationToken = default);
    Task<List<(StockReservation Reservation, int RemainingSeconds)>> GetReservationsForCartAsync(string cartToken, CancellationToken cancellationToken = default);
}