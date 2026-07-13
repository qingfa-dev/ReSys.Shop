using Module.Ordering.Domain.Adjustments;
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

            // Update: Set shipping method on cart.
            cart.ShippingMethodId = command.Request.ShippingMethodId;
            cart.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Compute: Calculate total order weight from variant weights.
            var variantIds = cart.LineItems.Select(li => li.VariantId).Distinct().ToList();
            var variantWeights = await dbContext.Set<Catalog.Domain.Products.Variants.Variant>()
                .Where(v => variantIds.Contains(v.Id))
                .Select(v => new { v.Id, v.Weight })
                .ToListAsync(cancellationToken);

            var weightMap = variantWeights.ToDictionary(v => v.Id, v => v.Weight ?? 0m);
            var orderWeight = cart.LineItems.Sum(li =>
                weightMap.TryGetValue(li.VariantId, out var w) ? li.Quantity * w : 0m);

            // Compute: Calculate shipping cost for the selected method.
            var calcResult = await ShippingRateCalculator.CalculateAsync(
                dbContext,
                command.Request.ShippingMethodId,
                orderWeight,
                cart.Total,
                cancellationToken);

            if (calcResult.IsSuccess)
            {
                var (cost, _) = calcResult.Value;

                // Remove: Clear old shipping adjustments before adding replacement.
                var existingShipping = cart.Adjustments
                    .Where(a => a.SourceType == AdjustmentConstant.SourceTypes.Shipping)
                    .ToList();
                foreach (var adj in existingShipping)
                {
                    cart.Adjustments.Remove(adj);
                    dbContext.Set<Adjustment>().Remove(adj);
                }

                // Create: Add shipping adjustment with computed cost.
                if (cost > 0)
                {
                    var adjResult = AdjustmentMethod.Create(
                        label: AdjustmentConstant.Labels.Shipping,
                        amount: cost,
                        adjustableId: cart.Id,
                        adjustableType: AdjustmentConstant.AdjustableTypes.Order,
                        sourceId: command.Request.ShippingMethodId,
                        sourceType: AdjustmentConstant.SourceTypes.Shipping,
                        orderId: cart.Id);

                    if (adjResult.IsSuccess)
                    {
                        cart.Adjustments.Add(adjResult.Value);
                        dbContext.Set<Adjustment>().Add(adjResult.Value);
                    }
                }
            }

            // Compute: Recalculate order totals regardless of calculator result.
            var recalcResult = cart.RecalculateTotals();
            if (recalcResult.IsFailure)
                return recalcResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
