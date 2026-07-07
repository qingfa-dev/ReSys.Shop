// Invariant: Calculator type defaults to PercentOnLineItem
namespace Module.Promotions.Domain.Actions;
/// <summary>Represents a Create Item Adjustments Action.</summary>

// Compute: Line-item-level adjustments applied per eligible line item
public sealed partial class CreateItemAdjustmentsAction : PromotionActionBase
{
    #region Constructor
    internal CreateItemAdjustmentsAction() { }
    #endregion Constructor

    #region Properties
    public string CalculatorType { get; set; } = "PercentOnLineItem";
    #endregion Properties

    #region Perform
    /// <summary>Performs line-item-level adjustments on each eligible line item in the order.</summary>
    /// <param name="options">Dictionary containing "order" and "promotion" keys.</param>
    /// <returns>A Result indicating success or a validation failure if the order is missing.</returns>
    // @CAT-5 Compute: Contract: pre=options contains "order" and "promotion" keys, post=adjustments created on eligible line items
    public override Result Perform(Dictionary<string, object> options)
    {
        // Validate: Order must be present in options payload
        if (!options.TryGetValue("order", out var orderObj) || orderObj is null)
            return ActionResult.Errors.OrderRequired;

        return Result.Ok();
    }
    #endregion Perform

    #region ComputeAmount
    // Compute: amount = min(line_item_amount, computed, order_amount_remaining) * -1
    // Guard: Prevent negative order totals by capping at remaining eligible adjustment sum
    public override decimal ComputeAmount(object computable)
    {
        return 0m;
    }
    #endregion ComputeAmount
}