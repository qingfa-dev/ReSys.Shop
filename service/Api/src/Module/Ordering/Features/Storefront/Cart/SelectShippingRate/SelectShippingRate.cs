using Module.Catalog.Domain.Variants;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Calculators;

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
            var previousTotal = cart.Total;
            var methodResult = cart.SetShippingMethod(command.Request.ShippingMethodId);
            if (methodResult.IsFailure)
                return methodResult.Errors;

            // Compute: Calculate total order weight from variant weights.
            var variantIds = cart.LineItems.Select(li => li.VariantId).Distinct().ToList();
            var variantWeights = await dbContext.Set<Variant>()
                .Where(v => variantIds.Contains(v.Id))
                .Select(v => new { v.Id, v.Weight })
                .ToListAsync(cancellationToken);

            var weightMap = variantWeights.ToDictionary(v => v.Id, v => v.Weight ?? 0m);
            var totalWeight = cart.CalculateTotalWeight(weightMap);

            // Compute: Calculate shipping cost for the selected method.
            var calcResult = await ShippingRateCalculator.CalculateAsync(
                dbContext,
                command.Request.ShippingMethodId,
                totalWeight,
                cart.Total,
                cancellationToken);

            if (calcResult.IsFailure)
                return calcResult.Errors;

            var (cost, _) = calcResult.Value;
            var shippingResult = cart.ReplaceShippingAdjustment(cost, command.Request.ShippingMethodId);
            if (shippingResult.IsFailure)
                return shippingResult.Errors;

            cart.RegressCheckoutIfAmountChanged(previousTotal);

            // Advance: Address → Delivery only; later states either already passed Delivery or regressed here.
            if (cart.CheckoutState == CheckoutState.Address)
            {
                var stateResult = cart.AdvanceCheckoutState(CheckoutState.Delivery);
                if (stateResult.IsFailure)
                    return stateResult.Errors;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok(OrderResult.Success.ShippingRateSelected(cart.Id));
        }
    }
}