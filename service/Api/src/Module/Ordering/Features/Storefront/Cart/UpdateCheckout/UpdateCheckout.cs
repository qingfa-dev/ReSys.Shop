using Module.Ordering.Domain.Adjustments;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Calculators;

namespace Module.Ordering.Features.Storefront.Cart.UpdateCheckout;

    /// <summary>Handles UpdateCheckout feature.</summary>
    public static partial class UpdateCheckout
{
    public sealed record Command(Request Request) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            // Query: Retrieve data from database.
            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Include(x => x.Adjustments)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            var req = command.Request;
            var addressChanged = req.ShipAddressId.HasValue && req.ShipAddressId != cart.ShipAddressId;

            if (req.Email is not null) cart.Email = req.Email;
            if (req.BillAddressId.HasValue) cart.BillAddressId = req.BillAddressId;
            if (req.ShipAddressId.HasValue) cart.ShipAddressId = req.ShipAddressId;
            if (req.SpecialInstructions is not null) cart.SpecialInstructions = req.SpecialInstructions;
            cart.ModifiedAtUtc = DateTimeOffset.UtcNow;

            // Compute: If ship address changed and a shipping method is selected, recalculate shipping cost.
            if (addressChanged && cart.ShippingMethodId.HasValue)
            {
                var variantIds = cart.LineItems.Select(li => li.VariantId).Distinct().ToList();
                var variantWeights = await dbContext.Set<Catalog.Domain.Products.Variants.Variant>()
                    .Where(v => variantIds.Contains(v.Id))
                    .Select(v => new { v.Id, v.Weight })
                    .ToListAsync(cancellationToken);

                var weightMap = variantWeights.ToDictionary(v => v.Id, v => v.Weight ?? 0m);
                var orderWeight = cart.LineItems.Sum(li =>
                    weightMap.TryGetValue(li.VariantId, out var w) ? li.Quantity * w : 0m);

                var calcResult = await ShippingRateCalculator.CalculateAsync(
                    dbContext,
                    cart.ShippingMethodId.Value,
                    orderWeight,
                    cart.Total,
                    cancellationToken);

                if (calcResult.IsSuccess)
                {
                    var (cost, _) = calcResult.Value;

                    var existingShipping = cart.Adjustments
                        .Where(a => a.SourceType == "Shipping")
                        .ToList();
                    foreach (var adj in existingShipping)
                    {
                        cart.Adjustments.Remove(adj);
                        dbContext.Set<Adjustment>().Remove(adj);
                    }

                    if (cost > 0)
                    {
                        var adjResult = AdjustmentMethod.Create(
                            label: "Shipping",
                            amount: cost,
                            adjustableId: cart.Id,
                            adjustableType: "Order",
                            sourceId: cart.ShippingMethodId.Value,
                            sourceType: "Shipping",
                            orderId: cart.Id);

                        if (adjResult.IsSuccess)
                        {
                            cart.Adjustments.Add(adjResult.Value);
                            dbContext.Set<Adjustment>().Add(adjResult.Value);
                        }
                    }
                }
            }

            cart.RecalculateTotals();
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
