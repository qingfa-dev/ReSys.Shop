using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.EmptyCart;

/// <summary>Removes all line items and adjustments from the current user's draft cart, resetting it to empty.</summary>
public static partial class EmptyCart
{
    public sealed record Command : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Calls domain Empty logic to clear line items and adjustments, then recalculates and persists.</summary>
        /// <param name="command">The (empty) command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var userId = Guid.TryParse(currentUser.UserId, out var parsed) ? parsed : (Guid?)null;
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            if (userId is null && string.IsNullOrWhiteSpace(sessionId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Find the user's draft cart with line items and adjustments.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Include(x => x.Adjustments)
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return Result.Ok();

            // Update: Clear all cart contents via domain logic (removes line items and adjustments).
            var result = cart.Empty();
            if (result.IsFailure)
                return result.Errors;

            cart.RecalculateTotals();
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
