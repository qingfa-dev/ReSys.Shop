using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Orders.Services;

namespace Module.Ordering.Features.Storefront.Cart.AssociateCart;

/// <summary>Associates a guest cart with the currently authenticated user, merging line items.</summary>
public static partial class AssociateCartWithUser
{
    public class Request
    {
        public Guid GuestOrderId { get; init; }
    }

    public class Response
    {
        public Guid Id { get; init; }
        public int ItemCount { get; init; }
    }

    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : ICommandHandler<Command, Response>
    {
        /// <summary>Merges a guest cart into the authenticated user's cart, combining matching line items by variant.</summary>
        /// <param name="command">The command containing the guest order ID to associate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The merged cart response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var userId = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : Guid.Empty;
            if (userId == Guid.Empty)
                return (Result<Response>)OrderResult.Errors.UserNotAuthenticated;

            var sessionId = currentUser.SessionId;

            // Check: Find the guest cart scoped to the current session.
            var guestOrder = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == command.Request.GuestOrderId && o.UserId == null && o.SessionId == sessionId, cancellationToken);

            if (guestOrder is null)
                return (Result<Response>)OrderResult.Errors.NotFound(command.Request.GuestOrderId);

            // Check: Find existing user cart — may or may not exist.
            var userOrder = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft, cancellationToken);

            if (userOrder is null)
            {
                // Update: No existing user cart — reassign guest cart to authenticated user.
                guestOrder.UserId = userId;
                guestOrder.SessionId = null;
            }
            else
            {
                // Merge: Combine guest cart line items into user cart by variant.
                var merger = new OrderMerger(userOrder);
                merger.Merge(guestOrder, userId, discardMerged: true);
                // Remove: Delete the now-empty guest cart.
                dbContext.Set<Order>().Remove(guestOrder);
            }

            var targetOrder = userOrder ?? guestOrder;
            targetOrder.RecalculateTotals();
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response { Id = targetOrder.Id, ItemCount = targetOrder.ItemCount };
        }
    }
}
