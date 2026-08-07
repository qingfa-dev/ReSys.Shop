using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.ReleaseSingle;

public sealed class ReleaseSingleReservationCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<ReleaseSingleReservationCommand>
{
    public async Task<Result> Handle(
        ReleaseSingleReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Set<StockReservation>()
            .FirstOrDefaultAsync(
                r => r.Id == command.ReservationId
                     && r.State == ReservationState.Reserved,
                cancellationToken);

        if (reservation is null)
            return StockReservationResult.Errors.NotFound(command.ReservationId);

        var releaseResult = reservation.Release();
        if (releaseResult.IsFailure)
            return releaseResult.Errors;

        reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
