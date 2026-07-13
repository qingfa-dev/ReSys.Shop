using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
using Module.Ordering.Features.Admin.Orders.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.CreateCart;

/// <summary>Creates a new shopping cart for the current user or as a guest cart.</summary>
public static partial class CreateCart
{
    public sealed record Command : ICommand<Response>;
    public sealed record Response : OrderDetailResponse;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser) : ICommandHandler<Command, Response>
    {
        /// <summary>Returns the existing draft cart for the current user or creates and persists a new one.</summary>
        /// <param name="command">The (empty) command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The cart detail response.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            var userId = Guid.TryParse(currentUser.UserId, out var parsedId) ? parsedId : (Guid?)null;
            var storeId = Guid.Empty;
            var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

            // Check: Return existing draft cart if one already exists — avoids duplicates.
            var existingCart = await dbContext.Set<Order>()
                .FirstOrDefaultAsync(x => (x.UserId == userId || x.SessionId == sessionId) && x.Status == OrderStatus.Draft, cancellationToken);

            if (existingCart is not null)
                return Result<Response>.Ok(existingCart.MapToDetail<Response>());

            // Create: New draft cart with default currency and session tracking.
            var createResult = OrderMethod.Create(OrderConstant.Defaults.Currency, userId, storeId, sessionId: sessionId);
            if (createResult.IsFailure) return (Result<Response>)createResult.Errors;

            var order = createResult.Value;
            dbContext.Set<Order>().Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Created(order.MapToDetail<Response>());
        }
    }
}
