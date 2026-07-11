using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.ValidateCheckout;

/// <summary>Validates that the current user's draft cart has all required fields for checkout: items, addresses, shipping method, and email.</summary>
public static partial class ValidateCheckout
{
    public sealed record Command : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Checks the draft cart for required checkout fields — items, addresses, shipping method, email — and returns errors for any missing.</summary>
        /// <param name="command">The (empty) command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success if valid, validation errors otherwise.</returns>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Find the user's draft cart with line items.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            if (cart.LineItems.Count == 0)
                return OrderResult.Errors.EmptyOrderCannotFinalize;

            if (cart.BillAddressId is null || cart.ShipAddressId is null)
                return OrderResult.Errors.AddressRequired;

            if (cart.ShippingMethodId is null)
                return OrderResult.Errors.DeliveryMethodRequired;

            if (string.IsNullOrWhiteSpace(cart.Email))
                return OrderResult.Errors.EmailRequired;

            return Result.Ok();
        }
    }
}
