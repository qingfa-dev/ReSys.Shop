using System.Data;

using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

/// <summary>Handles reservation of stock for a cart item with configurable TTL.</summary>
public static partial class ReserveCartStock
{
    public sealed record Command(Request Request, string CartToken) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the reserve cart stock command.</summary>
        /// <param name="command">The command containing variant, quantity, and cart token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the reservation details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var variantId = command.Request.VariantId;
            var quantity = command.Request.Quantity;
            var stockLocationId = command.Request.StockLocationId!.Value;
            var cartToken = command.CartToken;
            var ttlMinutes = command.Request.TtlMinutes;

            if (quantity <= 0)
                return StockReservationResult.Errors.QuantityZero;

            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);

            try
            {
                var stockItem = await dbContext.Set<StockItem>()
                    .FromSqlRaw("SELECT * FROM inventory.stock_items WHERE variant_id = {0} AND stock_location_id = {1} FOR UPDATE",
                        variantId, stockLocationId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stockItem is null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return StockReservationResult.Errors.InsufficientStock;
                }

                var reserved = await dbContext.Set<StockReservation>()
                    .Where(r => r.VariantId == variantId
                                && r.StockLocationId == stockLocationId
                                && r.State == ReservationState.Reserved
                                && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
                    .SumAsync(r => r.Quantity, cancellationToken);

                var available = stockItem.CountOnHand - reserved;
                if (available < quantity)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return StockReservationResult.Errors.InsufficientStock;
                }

                var result = StockReservationMethod.Reserve(variantId, quantity, stockLocationId, null, ttlMinutes, cartToken: cartToken);
                if (result.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return result.Errors;
                }

                dbContext.Set<StockReservation>().Add(result.Value);
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new Response
                {
                    Id = result.Value.Id,
                    VariantId = result.Value.VariantId,
                    Quantity = result.Value.Quantity,
                    ExpiresAtUtc = result.Value.ExpiresAtUtc!.Value,
                    State = result.Value.State.ToString()
                };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
