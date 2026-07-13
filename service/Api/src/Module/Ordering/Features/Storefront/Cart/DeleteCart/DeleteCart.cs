using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.DeleteCart;

/// <summary>Soft-deletes the current user's draft cart by marking it as deleted, reclaiming the resource.</summary>
public static partial class DeleteCart
{
    public sealed record Command : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Finds the current user's draft cart and marks it as deleted with a timestamp.</summary>
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
                return Result.Ok();

            // Check: Find the user's draft cart by user ID or guest session.
            var cart = await dbContext.Set<Order>()
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return Result.Ok();

            // Update: Soft-delete the cart with timestamp — preserves record for audit.
            var deleteResult = cart.Delete(currentUser.UserName ?? "System");
            if (deleteResult.IsFailure)
                return deleteResult.Errors;
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(OrderResult.Success.Deleted(cart.Id));
        }
    }
}
