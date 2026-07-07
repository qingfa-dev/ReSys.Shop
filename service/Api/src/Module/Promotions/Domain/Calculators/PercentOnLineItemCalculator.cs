using BuildingBlocks.Calculators;
using Shared.Application.Domain.Concerns.DisplayMoney;

namespace Module.Promotions.Domain.Calculators;
/// <summary>Represents a Percent On Line Item Calculator.</summary>

// Invariant: Percent on line item calculation uses configured percentage
public partial class PercentOnLineItemCalculator : Calculator
{
    public static new string DescriptionText => "Percent Per Item";

    public decimal Percent { get; set; }
    public bool ApplyOnlyOnFullPricedItems { get; set; }

    public override string Description => DescriptionText;

    /// <summary>Computes a percent-based discount per line item, optionally restricted to full-priced items.</summary>
    /// <param name="calculable">The ILineItemComputable item to calculate against.</param>
    /// <returns>A Result containing the rounded percentage amount or a guard/not-applicable failure.</returns>
    // @CAT-5 Compute:
    public override Result<decimal> Compute(object calculable)
    {
        if (calculable is not ILineItemComputable item)
            return PromotionsCalculatorResult.Errors.NotApplicable;

        if (ApplyOnlyOnFullPricedItems && item.DiscountedAmount < item.Amount)
        {
            return PromotionsCalculatorResult.Errors.FullPricedItemGuard;
        }

        var computedAmount = DisplayMoney.RoundToTwoPlaces(item.Amount * Percent / 100m);

        if (computedAmount > item.Amount)
            return Result.Ok(item.Amount);

        return Result.Ok(computedAmount);
    }
}