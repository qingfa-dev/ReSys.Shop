using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Cart.Shared.Services;

namespace Module.Ordering.Features.Storefront.Cart.UpdateCheckout;

/// <summary>Updates checkout fields (email, addresses, instructions) and recalculates shipping cost when the ship address changes.</summary>
public static partial class UpdateCheckout
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        /// <summary>Applies partial updates to the draft cart and recalculates shipping when the ship address is changed.</summary>
        /// <param name="command">The command containing checkout fields to update.</param>
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
                .Include(x => x.LineItems)
                .Include(x => x.Adjustments)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            var req = command.Request;
            var addressChanged = req.ShipAddressId.HasValue && req.ShipAddressId != cart.ShipAddressId;

            // Update: Apply partial checkout field updates (email, addresses, instructions).
            var previousTotal = cart.Total;
            var updateResult = cart.UpdateDetails(
                req.Email, req.SpecialInstructions,
                req.BillAddressId, req.ShipAddressId, null);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            // Apply: Recalculate the authoritative shipping cost after an address change.
            if (addressChanged && cart.ShippingMethodId.HasValue)
            {
                var costResult = await ShippingCostApplier.ApplyAsync(
                    dbContext, cart, cart.ShippingMethodId.Value, cancellationToken);
                if (costResult.IsFailure)
                    return costResult.Errors;
            }

            var recalcResult = cart.RecalculateTotals();
            if (recalcResult.IsFailure)
                return recalcResult.Errors;

            cart.RegressCheckoutIfAmountChanged(previousTotal);

            // Re-pick: an address change at Payment regresses to Delivery so the
            // customer re-confirms shipping cost and re-selects a payment method.
            if (addressChanged && cart.CheckoutState == CheckoutState.PickPaymentMethod)
            {
                var regress = cart.RegressCheckoutState(CheckoutState.PickDeliveryMethod);
                if (regress.IsFailure)
                    return regress.Errors;
            }

            // Advance: Address → Delivery once both addresses are set (fresh checkout).
            if (cart.HasAddresses() && cart.CheckoutState == CheckoutState.Address)
            {
                var adv = cart.AdvanceCheckoutState(CheckoutState.PickDeliveryMethod);
                if (adv.IsFailure)
                    return adv.Errors;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(OrderResult.Success.CheckoutUpdated(cart.Id));
        }
    }
}