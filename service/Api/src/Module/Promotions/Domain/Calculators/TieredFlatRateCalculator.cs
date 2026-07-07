using System.Globalization;
using BuildingBlocks.Calculators;

namespace Module.Promotions.Domain.Calculators;
/// <summary>Represents a Tiered Flat Rate Calculator.</summary>

// Invariant: Tiered flat rate calculation uses configured tier thresholds
public partial class TieredFlatRateCalculator : Calculator
{
    public static new string DescriptionText => "Tiered Flat Rate";

    public decimal BaseAmount { get; set; }
    public Dictionary<decimal, decimal> Tiers { get; set; } = [];

    public override string Description => DescriptionText;

    /// <summary>Computes a tiered flat-rate discount based on the calculable amount.</summary>
    /// <param name="calculable">The amount to calculate against (ILineItemComputable or decimal).</param>
    /// <returns>A Result containing the tier flat rate, falling back to BaseAmount if no tier matches.</returns>
    // @CAT-5 Compute:
    public override Result<decimal> Compute(object calculable)
    {
        var amount = calculable switch
        {
            ILineItemComputable li => li.Amount,
            _ => Convert.ToDecimal(calculable, CultureInfo.InvariantCulture),
        };

        foreach (var threshold in Tiers.OrderByDescending(t => t.Key))
        {
            if (amount >= threshold.Key)
                return Result.Ok(threshold.Value);
        }

        return Result.Ok(BaseAmount);
    }
}