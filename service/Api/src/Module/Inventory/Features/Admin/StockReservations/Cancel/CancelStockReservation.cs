using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockReservations.Cancel;

/// <summary>Handles cancellation of a stock reservation.</summary>
public static partial class CancelStockReservation
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the cancel stock reservation command.</summary>
        /// <param name="command">The command containing the reservation identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the updated reservation state.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null

            // Check: Find the reservation by identifier.
            var reservation = await dbContext.Set<StockReservation>()
                .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

            if (reservation is null)
                return StockReservationResult.Errors.NotFound(command.Id);

            // Update: Release the reservation.
            reservation.State = ReservationState.Released;
            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated reservation state.
            return reservation.MapToDetail<Response>();
        }
    }
}
