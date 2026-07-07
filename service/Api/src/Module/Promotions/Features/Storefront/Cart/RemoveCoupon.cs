using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using Module.Promotions.Domain.OrderPromotions;

namespace Module.Promotions.Features.Storefront.Cart;
/// <summary>Removes all applied coupons from the current user's draft cart.</summary>
public static partial class RemoveCoupon
{
    public sealed record Command : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Handles removing coupons from the cart.</summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ok result.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: User must be authenticated.
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Query: Get the user's draft cart.
            var cart = await dbContext.Set<Order>()
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            // Query: Get all applied order promotions.
            var orderPromotions = await dbContext.Set<OrderPromotion>()
                .Where(op => op.OrderId == cart.Id)
                .ToListAsync(cancellationToken);

            // Remove: Clear all coupons from cart.
            dbContext.Set<OrderPromotion>().RemoveRange(orderPromotions);
            // Persist: Save changes.
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
