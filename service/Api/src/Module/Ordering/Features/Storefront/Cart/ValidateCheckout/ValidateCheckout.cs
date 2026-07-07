using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.Cart.ValidateCheckout;

    /// <summary>Handles ValidateCheckout feature.</summary>
    public static partial class ValidateCheckout
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
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Query: Retrieve data from database.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
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
