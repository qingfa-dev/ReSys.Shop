using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.ConsumeCartStockReservations;

public sealed class ConsumeCartStockReservationsCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<ConsumeCartStockReservationsCommand>
{
    public async Task<Result> Handle(
        ConsumeCartStockReservationsCommand command, CancellationToken cancellationToken)
    {
        var reservations = await dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == command.CartId.ToString()
                        && r.State == ReservationState.Reserved)
            .ToListAsync(cancellationToken);

        if (reservations.Count == 0)
            return StockReservationResult.Errors.NoActiveReservations;

        foreach (var reservation in reservations)
        {
            var stockItem = await dbContext.Set<StockItem>()
                .FirstOrDefaultAsync(
                    si => si.VariantId == reservation.VariantId
                          && si.StockLocationId == reservation.StockLocationId,
                    cancellationToken);

            if (stockItem is null)
                return StockReservationResult.Errors.StockItemNotFound(reservation.VariantId);

            var pickResult = stockItem.Pick(reservation.Quantity);
            if (pickResult.IsFailure)
                return pickResult.Errors;

            reservation.State = ReservationState.Fulfilled;
            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
