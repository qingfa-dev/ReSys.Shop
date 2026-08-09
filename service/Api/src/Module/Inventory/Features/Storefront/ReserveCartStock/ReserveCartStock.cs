using System.Data;

using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.ReserveCartStock;

public sealed class ReserveCartStockCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<ReserveCartStockCommand, ReserveCartStockResponse>
{
    public async Task<Result<ReserveCartStockResponse>> Handle(
        ReserveCartStockCommand command, CancellationToken cancellationToken)
    {
        var reservationIds = new List<Guid>();

        await using var transaction = await dbContext.BeginTransactionAsync(
            IsolationLevel.RepeatableRead, cancellationToken);

        try
        {
            foreach (var item in command.LineItems)
            {
                var stockItems = await dbContext.Set<StockItem>()
                    .Where(si => si.VariantId == item.VariantId && si.CountOnHand > 0)
                    .OrderByDescending(si => si.CountOnHand)
                    .ToListAsync(cancellationToken);

                var remaining = item.Quantity;

                foreach (var stockItem in stockItems)
                {
                    if (remaining <= 0) break;

                    var reserved = await dbContext.Set<StockReservation>()
                        .Where(r => r.VariantId == item.VariantId
                                    && r.StockLocationId == stockItem.StockLocationId
                                    && r.State == ReservationState.Reserved
                                    && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
                        .SumAsync(r => r.Quantity, cancellationToken);

                    var available = stockItem.CountOnHand - reserved;
                    if (available <= 0) continue;

                    var take = Math.Min(available, remaining);
                    if (take <= 0) continue;

                    var result = StockReservationMethod.Reserve(
                        item.VariantId,
                        take,
                        stockItem.StockLocationId,
                        null,
                        command.TtlMinutes,
                        cartToken: command.CartId.ToString());

                    if (result.IsFailure)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return result.Errors;
                    }

                    dbContext.Set<StockReservation>().Add(result.Value);
                    reservationIds.Add(result.Value.Id);
                    remaining -= take;
                }

                if (remaining > 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return StockReservationResult.Errors.InsufficientStock;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ReserveCartStockResponse
            {
                ReservationIds = reservationIds
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
