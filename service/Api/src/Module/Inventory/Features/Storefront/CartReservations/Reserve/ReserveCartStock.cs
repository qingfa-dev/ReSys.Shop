using System.Data;

using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

using Shared.Operational.Persistence.Transactions;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

/// <summary>Reserves stock for a cart item using a serializable transaction to prevent oversell.</summary>
public static partial class ReserveCartStock
{
    public sealed record Command(Request Request, string CartToken) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Locks the stock row, validates availability, creates the reservation, and commits.</summary>
        /// <param name="command">The command containing variant, quantity, and cart token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result with the reservation details.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null && command.Request.Quantity>0, post=result!=null, throws=DbUpdateException
            var variantId = command.Request.VariantId;
            var quantity = command.Request.Quantity;
            var stockLocationId = command.Request.StockLocationId!.Value;
            var cartToken = command.CartToken;
            var ttlMinutes = command.Request.TtlMinutes;

            if (quantity <= 0)
                return StockReservationResult.Errors.QuantityZero;

            await using var transaction = await dbContext.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);

            try
            {
                var stockItem = await dbContext.Set<StockItem>()
                    .FirstOrDefaultAsync(si => si.VariantId == variantId && si.StockLocationId == stockLocationId, cancellationToken);

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
