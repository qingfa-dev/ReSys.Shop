using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.RemoveItem;

    /// <summary>Handles RemoveCartItem feature.</summary>
    public static partial class RemoveCartItem
{
    public sealed record Command(Guid LineItemId) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Query: Retrieve data from database.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            var lineItem = cart.LineItems.FirstOrDefault(li => li.Id == command.LineItemId);
            if (lineItem is null)
                return LineItemResult.Errors.NotFound(command.LineItemId);

            // Remove: Detach entity from collection.
            cart.LineItems.Remove(lineItem);
            // Remove: Delete entity from database.
            dbContext.Set<Module.Ordering.Domain.LineItems.LineItem>().Remove(lineItem);
            cart.RecalculateTotals();
            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
