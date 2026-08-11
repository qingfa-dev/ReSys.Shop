using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.StockReservations.ConsumeCart;

public static partial class ConsumeCartStockReservations
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
}
