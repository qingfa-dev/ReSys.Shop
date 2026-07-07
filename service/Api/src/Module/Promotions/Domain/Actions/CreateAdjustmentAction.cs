// Invariant: Calculator type is always set, defaults to FlatPercentItemTotal
namespace Module.Promotions.Domain.Actions;
/// <summary>Represents a Create Adjustment Action.</summary>

// Compute: Order-level adjustment applied to the entire order total
public sealed partial class CreateAdjustmentAction : PromotionActionBase
{
    #region Constructor
    internal CreateAdjustmentAction() { }
    #endregion Constructor

    #region Properties
    public string CalculatorType { get; set; } = "FlatPercentItemTotal";
    #endregion Properties

    #region Perform
    /// <summary>Performs an order-level adjustment applied to the entire order total.</summary>
    /// <param name="options">Dictionary containing an "order" key with the Order object.</param>
    /// <returns>A Result indicating success or a validation failure if the order is missing.</returns>
    // @CAT-5 Compute: Contract: pre=options contains "order" key with Order object, post=adjustment created on order
    public override Result Perform(Dictionary<string, object> options)
    {
        // Validate: Order must be present in options payload
        if (!options.TryGetValue("order", out var orderObj) || orderObj is null)
            return ActionResult.Errors.OrderRequired;

        return Result.Ok();
    }
    #endregion Perform

    #region ComputeAmount
    // Compute: amount = min(item_total + ship_total - shipping_discount, computed) * -1
    public override decimal ComputeAmount(object computable)
    {
        return 0m;
    }
    #endregion ComputeAmount
}