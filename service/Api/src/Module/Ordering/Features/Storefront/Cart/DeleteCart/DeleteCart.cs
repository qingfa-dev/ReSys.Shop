using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.DeleteCart;

    /// <summary>Handles DeleteCart feature.</summary>
    public static partial class DeleteCart
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
                return Result.Ok();

            // Query: Retrieve data from database.
            var cart = await dbContext.Set<Order>()
                .Where(x => (x.UserId == userId && x.Status == OrderStatus.Draft)
                         || (x.SessionId == sessionId && x.Status == OrderStatus.Draft))
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return Result.Ok();

            // Update: Modify entity properties.
            cart.IsDeleted = true;
            // Update: Modify entity properties.
            cart.DeletedAtUtc = DateTimeOffset.UtcNow;
            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
