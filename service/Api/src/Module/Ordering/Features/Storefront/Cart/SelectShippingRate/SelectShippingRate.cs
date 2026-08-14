using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Services;

namespace Module.Ordering.Features.Storefront.Cart.SelectShippingRate;

/// <summary>Selects a shipping method for the cart, calculates the shipping cost, replaces existing shipping adjustments, and recalculates totals.</summary>
public static partial class SelectShippingRate
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Sets the shipping method, calculates cost from weight, replaces old shipping adjustments, and persists.</summary>
        /// <param name="command">The command containing the shipping method ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="DbUpdateException">Thrown when the database update fails.</exception>
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null, throws=DbUpdateException
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Check: Find the user's draft cart with line items and adjustments.
            var cart = await dbContext.Set<Order>()
                .Include(o => o.LineItems)
                .Include(o => o.Adjustments)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            // Update: Set shipping method on cart via domain method.
            var previousMethodId = cart.ShippingMethodId;
            var previousTotal = cart.Total;
            var methodResult = cart.SetShippingMethod(command.Request.ShippingMethodId);
            if (methodResult.IsFailure)
                return methodResult.Errors;

            // Apply: Authoritative server-side shipping cost for the selected method.
            var costResult = await ShippingCostApplier.ApplyAsync(
                dbContext, cart, command.Request.ShippingMethodId, cancellationToken);
            if (costResult.IsFailure)
                return costResult.Errors;

            cart.RegressCheckoutIfAmountChanged(previousTotal);

            // Re-pick: a shipping method change at Payment regresses to Delivery so the
            // customer re-selects a payment method against the new shipping cost.
            if (cart.CheckoutState == CheckoutState.PickPaymentMethod
                && command.Request.ShippingMethodId != previousMethodId)
            {
                var regress = cart.RegressCheckoutState(CheckoutState.PickDeliveryMethod);
                if (regress.IsFailure)
                    return regress.Errors;
            }

            // Advance: Address → Delivery only; later states either already passed Delivery or regressed here.
            if (cart.CheckoutState == CheckoutState.Address)
            {
                var stateResult = cart.AdvanceCheckoutState(CheckoutState.PickDeliveryMethod);
                if (stateResult.IsFailure)
                    return stateResult.Errors;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok(OrderResult.Success.ShippingRateSelected(cart.Id));
        }
    }
}