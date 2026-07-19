using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Release;

public static partial class ReleaseCartReservation
{
    public sealed record Command(Guid ReservationId) : ICommand;

    /// <summary>Handler for releasing a cart reservation.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        /// <summary>Releases a cart reservation.</summary>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var reservation = await dbContext.Set<StockReservation>()
                .FirstOrDefaultAsync(r => r.Id == command.ReservationId, cancellationToken);

            if (reservation is null)
                return StockReservationResult.Errors.NotFound(command.ReservationId);

            var releaseResult = reservation.Release();
            if (releaseResult.IsFailure) return releaseResult;

            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

            if (reservation.StockLocationId is not null)
            {
                var stockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si =>
                        si.VariantId == reservation.VariantId &&
                        si.StockLocationId == reservation.StockLocationId.Value,
                        cancellationToken);

                if (stockItem is not null)
                    stockItem.CountOnHand += reservation.Quantity;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(StockReservationResult.Success.Released(reservation.Id));
        }
    }
}