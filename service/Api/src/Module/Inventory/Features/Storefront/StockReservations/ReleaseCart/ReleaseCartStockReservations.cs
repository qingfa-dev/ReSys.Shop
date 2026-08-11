using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.StockReservations.ReleaseCart;

public static partial class ReleaseCartStockReservations
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(
            Command command, CancellationToken cancellationToken)
        {
            var reservations = await dbContext.Set<StockReservation>()
                .Where(r => r.CartToken == command.Request.CartId.ToString()
                            && r.State == ReservationState.Reserved)
                .ToListAsync(cancellationToken);

            foreach (var reservation in reservations)
            {
                var releaseResult = reservation.Release();
                if (releaseResult.IsFailure)
                    return releaseResult.Errors;

                reservation.ModifiedAtUtc = DateTimeOffset.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
