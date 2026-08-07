using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.ReleaseSingle;

public static partial class ReleaseCartReservation
{
    public sealed record Command(Request Request) : ICommand<Response>;

    /// <summary>Handler for releasing a cart reservation.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Releases a cart reservation.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var reservation = await dbContext.Set<StockReservation>()
                .FirstOrDefaultAsync(r => r.Id == command.Request.ReservationId
                                       && r.CartToken == command.Request.CartToken, cancellationToken);

            if (reservation is null)
                return StockReservationResult.Errors.NotFound(command.Request.ReservationId);

            var releaseResult = reservation.Release();
            if (releaseResult.IsFailure) return releaseResult.Errors;

            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                ReservationId = reservation.Id,
                Status = "released"
            };
        }
    }
}
