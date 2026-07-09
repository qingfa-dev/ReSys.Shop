using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Services;

public class StockReservationService(IApplicationDbContext dbContext) : IStockReservationService
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<StockReservation>> ReserveAsync(
        Guid variantId,
        int quantity,
        Guid stockLocationId,
        Guid orderId,
        int ttlMinutes = 30,
        CancellationToken cancellationToken = default)
    {
        var result = StockReservationMethod.Reserve(variantId, quantity, stockLocationId, orderId, ttlMinutes);
        if (result.IsFailure) return result;

        var reservation = result.Value;
        _dbContext.Set<StockReservation>().Add(reservation);

        return reservation;
    }

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

    public async Task<int> ExpireReservationsAndRestoreStockAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expired = await _dbContext.Set<StockReservation>()
            .Where(r => r.State == ReservationState.Reserved && r.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var r in expired)
        {
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
}
