using BuildingBlocks.Calculators;

namespace Module.Promotions.Domain.Calculators;
/// <summary>Represents a Flexi Rate Calculator.</summary>

// Invariant: Flexi rate calculation uses configured flexi parameters
public partial class FlexiRateCalculator : Calculator
{
    public static new string DescriptionText => "Flexible Rate";

    public decimal FirstItem { get; set; }
    public decimal AdditionalItem { get; set; }
    public int MaxItems { get; set; }
    public string Currency { get; set; } = PromotionsCalculatorConstant.Defaults.Currency;
    public bool ApplyOnlyOnFullPricedItems { get; set; }

    public override string Description => DescriptionText;

    /// <summary>Computes a flexible rate discount based on quantity tiers (first item + additional items).</summary>
    /// <param name="calculable">The ILineItemComputable item to calculate against.</param>
    /// <returns>A Result containing the computed amount or a guard/not-applicable failure.</returns>
    // @CAT-5 Compute:
    public override Result<decimal> Compute(object calculable)
    {
        if (calculable is not ILineItemComputable item)
            return PromotionsCalculatorResult.Errors.NotApplicable;

        if (ApplyOnlyOnFullPricedItems && item.DiscountedAmount < item.Amount)
        {
            return PromotionsCalculatorResult.Errors.FullPricedItemGuard;
        }

        return Result.Ok(ComputeFromQuantity(item.Quantity));
    }

    public decimal ComputeFromQuantity(int quantity)
    {
        var filtered = new[] { quantity, MaxItems }.Where(x => x > 0).ToArray();

        if (filtered.Length == 0)
            return 0m;

        var count = filtered.Min();
        return FirstItem + (count - 1) * AdditionalItem;
    }
}