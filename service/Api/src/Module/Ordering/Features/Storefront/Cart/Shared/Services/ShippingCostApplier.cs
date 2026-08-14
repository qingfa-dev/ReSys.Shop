using Module.Catalog.Domain.Variants;
using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Calculators;

namespace Module.Ordering.Features.Storefront.Cart.Shared.Services;

/// <summary>Applies the authoritative shipping cost to a cart/order: computes total weight, calls the shipping-rate calculator, and replaces the existing shipping adjustment.</summary>
public static class ShippingCostApplier
{
    /// <summary>Computes the shipping cost for the given method against the cart's weight and total, then replaces the shipping adjustment.</summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="cart">The draft order (cart) — must have LineItems and Adjustments loaded.</param>
    /// <param name="shippingMethodId">The shipping method to price.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of replacing the shipping adjustment.</returns>
    public static async Task<Result> ApplyAsync(
        IApplicationDbContext dbContext,
        Order cart,
        Guid shippingMethodId,
        CancellationToken cancellationToken)
    {
        // Compute: Total order weight from variant weights.
        var variantIds = cart.LineItems.Select(li => li.VariantId).Distinct().ToList();
        var variantWeights = await dbContext.Set<Variant>()
            .Where(v => variantIds.Contains(v.Id))
            .Select(v => new { v.Id, v.Weight })
            .ToListAsync(cancellationToken);

        var weightMap = variantWeights.ToDictionary(v => v.Id, v => v.Weight ?? 0m);
        var totalWeight = cart.CalculateTotalWeight(weightMap);

        // Compute: Authoritative shipping cost for the selected method.
        var calcResult = await ShippingRateCalculator.CalculateAsync(
            dbContext, shippingMethodId, totalWeight, cart.Total, cancellationToken);
        if (calcResult.IsFailure)
            return calcResult.Errors;

        var (cost, _) = calcResult.Value;
        return cart.ReplaceShippingAdjustment(cost, shippingMethodId);
    }
}
