using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;

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
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var userId = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : Guid.Empty;
            if (userId == Guid.Empty)
                return (Result<Response>)OrderResult.Errors.UserNotAuthenticated;

            var sessionId = currentUser.SessionId;

            var guestOrder = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == command.Request.GuestOrderId && o.UserId == null && o.SessionId == sessionId, cancellationToken);

            if (guestOrder is null)
                return (Result<Response>)OrderResult.Errors.NotFound(command.Request.GuestOrderId);

            var userOrder = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Draft, cancellationToken);

            if (userOrder is null)
            {
                // No existing user cart — assign guest cart to user
                guestOrder.UserId = userId;
                guestOrder.SessionId = null;
            }
            else
            {
                // Merge guest cart into user cart
                var merger = new OrderMerger(userOrder);
                merger.Merge(guestOrder, userId, discardMerged: true);
                dbContext.Set<Order>().Remove(guestOrder);
            }

            var targetOrder = userOrder ?? guestOrder;
            targetOrder.RecalculateTotals();
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response { Id = targetOrder.Id, ItemCount = targetOrder.LineItems.Count };
        }
    }
}
