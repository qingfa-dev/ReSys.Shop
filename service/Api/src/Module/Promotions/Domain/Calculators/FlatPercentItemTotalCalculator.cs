using BuildingBlocks.Calculators;
using Shared.Application.Domain.Concerns.DisplayMoney;

namespace Module.Promotions.Domain.Calculators;
/// <summary>Represents a Flat Percent Item Total Calculator.</summary>

// Invariant: Flat percent item total calculation uses configured percentage
public partial class FlatPercentItemTotalCalculator : Calculator
{
    public static new string DescriptionText => "Flat Percent";

    public decimal FlatPercent { get; set; }

    public override string Description => DescriptionText;

    /// <summary>Computes a flat percent discount on the item total, capped at the item amount.</summary>
    /// <param name="calculable">The ILineItemComputable item to calculate against.</param>
    /// <returns>A Result containing the rounded percentage amount or a NotApplicable failure.</returns>
    // @CAT-5 Compute:
    public override Result<decimal> Compute(object calculable)
    {
        if (calculable is not ILineItemComputable item)
            return PromotionsCalculatorResult.Errors.NotApplicable;

        var computedAmount = DisplayMoney.RoundToTwoPlaces(item.Amount * FlatPercent / 100m);

        if (computedAmount > item.Amount)
            return Result.Ok(item.Amount);

        return Result.Ok(computedAmount);
    }
}