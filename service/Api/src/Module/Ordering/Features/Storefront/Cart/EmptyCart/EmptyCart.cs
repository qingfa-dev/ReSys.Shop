using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.EmptyCart;

    /// <summary>Handles EmptyCart feature.</summary>
    public static partial class EmptyCart
{
    public sealed record Command : ICommand;

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
            var userId = Guid.TryParse(currentUser.UserId, out var parsed) ? parsed : (Guid?)null;
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            if (userId is null && string.IsNullOrWhiteSpace(sessionId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Query: Retrieve data from database.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Include(x => x.Adjustments)
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return Result.Ok();

            // Update: Clear entity contents.
            var result = cart.Empty();
            if (result.IsFailure)
                return result.Errors;

            cart.RecalculateTotals();
            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
