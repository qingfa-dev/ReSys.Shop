using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Release;

public static partial class ReleaseCartReservation
{
    public sealed record Command(Guid ReservationId) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var reservation = await dbContext.Set<StockReservation>()
                .FirstOrDefaultAsync(r => r.Id == command.ReservationId, cancellationToken);

            if (reservation is null)
                return StockReservationResult.Errors.NotFound(command.ReservationId);

            if (reservation.State != ReservationState.Reserved)
                return StockReservationResult.Errors.InvalidStateTransition;

            reservation.State = ReservationState.Released;
            reservation.ExpiresAtUtc = DateTimeOffset.UtcNow;
            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(StockReservationResult.Success.Released(reservation.Id));
        }
    }
}