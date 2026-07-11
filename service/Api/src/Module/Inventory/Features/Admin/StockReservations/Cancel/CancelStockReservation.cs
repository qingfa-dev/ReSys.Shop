using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockReservations.Cancel;

/// <summary>Releases a stock reservation by transitioning its state to Released.</summary>
public static partial class CancelStockReservation
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Finds the reservation and marks it as released.</summary>
        /// <param name="command">The command containing the reservation identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the updated reservation state.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException

            // Check: Find the reservation by identifier.
            var reservation = await dbContext.Set<StockReservation>()
                .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

            if (reservation is null)
                return StockReservationResult.Errors.NotFound(command.Id);

            // Update: Release the reservation.
            reservation.State = ReservationState.Released;
            reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the updated reservation state.
            return reservation.MapToDetail<Response>();
        }
    }
}
