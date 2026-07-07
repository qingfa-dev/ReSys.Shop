using BuildingBlocks.Calculators;

namespace Module.Promotions.Domain.Calculators;
/// <summary>Represents a Flat Rate Calculator.</summary>

// Invariant: Flat rate calculation returns configured amount
public partial class FlatRateCalculator : Calculator
{
    public static new string DescriptionText => "Flat Rate Per Order";

    public decimal Amount { get; set; }
    public string Currency { get; set; } = PromotionsCalculatorConstant.Defaults.Currency;
    public bool ApplyOnlyOnFullPricedItems { get; set; }

    public override string Description => DescriptionText;

    /// <summary>Computes a flat-rate discount per order, optionally restricted to full-priced items.</summary>
    /// <param name="calculable">The ILineItemComputable item or object to calculate against.</param>
    /// <returns>A Result containing the flat amount or a guard/not-applicable failure.</returns>
    // @CAT-5 Compute:
    public override Result<decimal> Compute(object calculable)
    {
        if (ApplyOnlyOnFullPricedItems && calculable is ILineItemComputable item
            && item.DiscountedAmount < item.Amount)
        {
            return PromotionsCalculatorResult.Errors.FullPricedItemGuard;
        }

        if (calculable is ILineItemComputable li
            && Currency.Equals(li.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Ok(Amount);
        }

        return PromotionsCalculatorResult.Errors.NotApplicable;
    }
}