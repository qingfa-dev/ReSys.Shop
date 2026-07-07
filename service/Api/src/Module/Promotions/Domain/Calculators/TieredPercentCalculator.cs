using System.Globalization;
using BuildingBlocks.Calculators;
using Shared.Application.Domain.Concerns.DisplayMoney;

namespace Module.Promotions.Domain.Calculators;
/// <summary>Represents a Tiered Percent Calculator.</summary>

// Invariant: Tiered percent calculation uses configured tier thresholds
public partial class TieredPercentCalculator : Calculator
{
    public static new string DescriptionText => "Tiered Percent";

    public decimal BasePercent { get; set; }
    public Dictionary<decimal, decimal> Tiers { get; set; } = [];

    public override string Description => DescriptionText;

    /// <summary>Computes a tiered percent discount based on the calculable amount.</summary>
    /// <param name="calculable">The amount to calculate against (ILineItemComputable or decimal).</param>
    /// <returns>A Result containing the rounded percentage amount, falling back to BasePercent if no tier matches.</returns>
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
                return Result.Ok(DisplayMoney.RoundToTwoPlaces(amount * threshold.Value / 100m));
        }

        return Result.Ok(DisplayMoney.RoundToTwoPlaces(amount * BasePercent / 100m));
    }
}