using System.Globalization;
using BuildingBlocks.Calculators;

namespace Module.Promotions.Domain.Calculators;
/// <summary>Represents a Price Sack Calculator.</summary>

// Invariant: Price sack calculation uses configured range thresholds
public partial class PriceSackCalculator : Calculator
{
    public static new string DescriptionText => "Price Sack";

    public decimal MinimalAmount { get; set; }
    public decimal NormalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Currency { get; set; } = PromotionsCalculatorConstant.Defaults.Currency;

    public override string Description => DescriptionText;

    /// <summary>Computes a price-sack discount: NormalAmount below MinimalAmount, DiscountAmount otherwise.</summary>
    /// <param name="calculable">The amount to evaluate (ILineItemComputable, IEnumerable, or decimal).</param>
    /// <returns>A Result containing NormalAmount or DiscountAmount based on the threshold.</returns>
    // @CAT-5 Compute:
    public override Result<decimal> Compute(object calculable)
    {
        var baseAmount = calculable switch
        {
            IEnumerable<object> items => items
                .Select(o => o is ILineItemComputable li ? li.Amount : Convert.ToDecimal(o, CultureInfo.InvariantCulture))
                .Sum(),
            ILineItemComputable li => li.Amount,
            _ => Convert.ToDecimal(calculable, CultureInfo.InvariantCulture),
        };

        if (baseAmount < MinimalAmount)
            return Result.Ok(NormalAmount);

        return Result.Ok(DiscountAmount);
    }
}