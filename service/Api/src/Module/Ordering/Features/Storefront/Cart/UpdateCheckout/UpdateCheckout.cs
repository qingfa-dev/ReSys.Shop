using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Calculators;

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
            var updateResult = cart.UpdateDetails(
                req.Email, req.SpecialInstructions,
                req.BillAddressId, req.ShipAddressId, null);
            if (updateResult.IsFailure)
                return updateResult.Errors;

            // Compute: Recalculate shipping cost when ship address changes and a method is selected.
            if (addressChanged && cart.ShippingMethodId.HasValue)
            {
                var variantIds = cart.LineItems.Select(li => li.VariantId).Distinct().ToList();
                var variantWeights = await dbContext.Set<Catalog.Domain.Products.Variants.Variant>()
                    .Where(v => variantIds.Contains(v.Id))
                    .Select(v => new { v.Id, v.Weight })
                    .ToListAsync(cancellationToken);

                var weightMap = variantWeights.ToDictionary(v => v.Id, v => v.Weight ?? 0m);
                var totalWeight = cart.CalculateTotalWeight(weightMap);

                var calcResult = await ShippingRateCalculator.CalculateAsync(
                    dbContext,
                    cart.ShippingMethodId.Value,
                    totalWeight,
                    cart.Total,
                    cancellationToken);

                if (calcResult.IsSuccess)
                {
                    var (cost, _) = calcResult.Value;

                    var shippingResult = cart.ReplaceShippingAdjustment(cost, cart.ShippingMethodId.Value);
                    if (shippingResult.IsFailure)
                        return shippingResult.Errors;
                }
            }

            var recalcResult = cart.RecalculateTotals();
            if (recalcResult.IsFailure)
                return recalcResult.Errors;

            var stateResult = cart.AdvanceCheckoutState(CheckoutState.Address);
            if (stateResult.IsFailure)
                return stateResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(OrderResult.Success.CheckoutUpdated(cart.Id));
        }
    }
}