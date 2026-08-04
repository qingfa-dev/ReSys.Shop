using Module.Inventory.Domain.StockReservations;
using Shared.Application.Contracts.Inventory;

namespace Module.Inventory.Features.Storefront.Contracts.ReleaseCartStockReservations;

public sealed class ReleaseCartStockReservationsCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<ReleaseCartStockReservationsCommand>
{
    public async Task<Result> Handle(
        ReleaseCartStockReservationsCommand command, CancellationToken cancellationToken)
    {
        var reservations = await dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == command.CartId.ToString()
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
