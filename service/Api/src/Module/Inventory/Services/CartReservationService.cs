using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services.Abstractions;

namespace Module.Inventory.Services;

public class CartReservationService(IApplicationDbContext dbContext) : ICartReservationService
{
    private readonly IApplicationDbContext _dbContext = dbContext;

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

        var stockItem = await _dbContext.Set<StockItem>()
            .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

        if (stockItem is null) return StockReservationResult.Errors.InsufficientStock;

        var reserved = await _dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.StockLocationId == stockLocationId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .SumAsync(r => r.Quantity, cancellationToken);

        var available = stockItem.CountOnHand - reserved;
        if (available < quantity)
            return StockReservationResult.Errors.InsufficientStock;

        var result = StockReservationMethod.Reserve(variantId, quantity, stockLocationId, null, ttlMinutes, cartToken: cartToken);
        if (result.IsFailure) return result;

        var reservation = result.Value;
        _dbContext.Set<StockReservation>().Add(reservation);

        return reservation;
    }

    public async Task ReleaseCartReservationsAsync(string cartToken, CancellationToken cancellationToken = default)
    {
        var reservations = await _dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == cartToken && r.State == ReservationState.Reserved)
            .ToListAsync(cancellationToken);

        foreach (var r in reservations)
        {
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
}
