using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Calculators;

namespace Module.Ordering.Features.Storefront.Cart.SelectShippingRate;

    /// <summary>Handles SelectShippingRate feature.</summary>
    public static partial class SelectShippingRate
{
    public sealed record Command(Request Request) : ICommand;

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
                .Include(o => o.LineItems)
                .Include(o => o.Adjustments)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            // Update: Set shipping method on cart.
            cart.ShippingMethodId = command.Request.ShippingMethodId;
            cart.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Compute: Calculate order weight from line items.
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

                // Remove existing shipping adjustments to avoid duplicates.
                var existingShipping = cart.Adjustments
                    .Where(a => a.SourceType == "Shipping")
                    .ToList();
                foreach (var adj in existingShipping)
                {
                    cart.Adjustments.Remove(adj);
                    dbContext.Set<Adjustment>().Remove(adj);
                }

                // Create: Add shipping adjustment with computed cost.
                if (cost > 0)
                {
                    var adjustment = AdjustmentMethod.Create(
                        label: "Shipping",
                        amount: cost,
                        adjustableId: cart.Id,
                        adjustableType: "Order",
                        sourceId: command.Request.ShippingMethodId,
                        sourceType: "Shipping",
                        orderId: cart.Id).Value;

                    cart.Adjustments.Add(adjustment);
                    dbContext.Set<Adjustment>().Add(adjustment);
                }

                // Compute: Recalculate order totals.
                cart.RecalculateTotals();
            }

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
