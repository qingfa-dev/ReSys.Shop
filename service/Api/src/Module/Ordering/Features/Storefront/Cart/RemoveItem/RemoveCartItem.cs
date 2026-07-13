using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.RemoveItem;

/// <summary>Removes a line item from the current user's draft cart and recalculates order totals.</summary>
public static partial class RemoveCartItem
{
    public sealed record Command(Guid LineItemId) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Finds the user's draft cart, removes the specified line item, and recalculates totals.</summary>
        /// <param name="command">The command containing the line item ID to remove.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Find the user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            // Check: Find the line item within the cart's collection.
            var lineItem = cart.LineItems.FirstOrDefault(li => li.Id == command.LineItemId);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Remove: Use domain method to remove from collection and recalculate, then delete from database for EF tracking.
            var removeResult = cart.RemoveLineItem(command.LineItemId);
            if (removeResult.IsFailure)
                return removeResult.Errors;

            dbContext.Set<LineItem>().Remove(removeResult.Value);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
