using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Domain.ShippingRates;

namespace Module.Shipping.Domain.Calculators;

/// <summary>Calculates the best shipping rate for an order based on weight and total.</summary>
// @CAT-10 Boundary: Domain → Data — queries ShippingRate via IApplicationDbContext
public static class ShippingRateCalculator
{
    /// <summary>
    /// Calculates the shipping cost for a given method, considering order weight and total.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="shippingMethodId">The selected shipping method.</param>
    /// <param name="orderWeight">Total order weight in the rate's weight unit.</param>
    /// <param name="orderTotal">Total order amount (for free-shipping threshold).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result with (cost, isFree) tuple on success, or failure on no rate available.</returns>
    // @CAT-10 Contract: pre=dbContext!=null, post=cost>=0, throws=none
    public static async Task<Result<(decimal cost, bool isFree)>> CalculateAsync(
        IApplicationDbContext dbContext,
        Guid shippingMethodId,
        decimal orderWeight,
        decimal orderTotal,
        CancellationToken cancellationToken = default)
    {
        // Load: Get all rates for the shipping method, ordered by cost ascending.
        var rates = await dbContext.Set<ShippingRate>()
            .Where(r => r.ShippingMethodId == shippingMethodId)
            .OrderBy(r => r.Cost)
            .ToListAsync(cancellationToken);

        // Guard: No rates available at all.
        if (rates.Count == 0)
            return ShippingMethodResult.Errors.NoRateAvailable;

        // Filter: Find rates that match the order weight.
        var weightMatchRates = rates
            .Where(r =>
                // No weight restriction — matches any weight
                (r.MinWeight == null && r.MaxWeight == null)
                // Weight-bound match
                || ((r.MinWeight == null || r.MinWeight <= orderWeight)
                    && (r.MaxWeight == null || r.MaxWeight >= orderWeight)))
            .ToList();

        // If no weight-match, fall back to unrestricted-weight rates.
        if (weightMatchRates.Count == 0)
        {
            weightMatchRates = rates
                .Where(r => r.MinWeight == null && r.MaxWeight == null)
                .ToList();
        }

        // If still no match, fall back to cheapest available rate.
        var candidateRates = weightMatchRates.Count > 0 ? weightMatchRates : rates;
        if (candidateRates.Count == 0)
            return ShippingMethodResult.Errors.NoRateAvailable;

        // Check: Free-shipping threshold — check weight-matching rates first.
        var freeRate = weightMatchRates
            .FirstOrDefault(r => r.FreeShippingThreshold.HasValue && orderTotal >= r.FreeShippingThreshold.Value);

        if (freeRate is not null)
            return (0m, true);

        // Compute: Select the cheapest matching rate.
        var bestRate = candidateRates[0];
        return (bestRate.Cost, false);
    }
}
