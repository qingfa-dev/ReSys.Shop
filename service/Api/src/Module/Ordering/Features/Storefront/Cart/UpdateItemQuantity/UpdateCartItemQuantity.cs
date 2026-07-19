using Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;
/// <summary>Updates the quantity of a line item in the current user's draft cart after validating stock availability.</summary>
public static partial class UpdateCartItemQuantity
{
    public sealed record Command(Guid LineItemId, Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser,
        ISender sender)
        : ICommandHandler<Command>
    {
        /// <summary>Validates stock via Inventory module, updates the line item quantity and total, and recalculates cart totals.</summary>
        /// <param name="command">The command containing the line item ID and new quantity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            if (command.Request.Quantity <= 0 || command.Request.Quantity > LineItemConstant.MaxQuantity)
                return OrderResult.Errors.QuantityNotPositive;

            // Check: Find the user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            var lineItem = cart.LineItems.FirstOrDefault(li => li.Id == command.LineItemId);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Validate: Stock availability via Inventory module's reservation-aware query.
            var stockResult = await sender.Send(
                new CheckStockAvailability.Query(new CheckStockAvailability.Request
                {
                    VariantId = lineItem.VariantId,
                    Quantity = command.Request.Quantity
                }),
                cancellationToken);

            if (!stockResult.Value.IsAvailable)
                return OrderResult.Errors.CartQuantityInvalid;

            // Update: Modify quantity and total.
            var updateResult = lineItem.UpdateQuantity(command.Request.Quantity);
            if (updateResult.IsFailure)
                return updateResult.Errors;
            var recalcResult = cart.RecalculateTotals();
            if (recalcResult.IsFailure)
                return recalcResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record quantity change in audit log.
            LineItemLoggers.QuantityUpdated(logger, Id: lineItem.Id, OrderId: cart.Id, Quantity: lineItem.Quantity, ActionBy: currentUser.UserName);

            return Result.Ok();
        }
    }
}
