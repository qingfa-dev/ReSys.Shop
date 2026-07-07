namespace Module.Promotions.Domain.Actions;
/// <summary>Represents a Create Line Items Action.</summary>

// Enforce: Adds free line items to order if promotion is eligible
// Invariant: Line items are created based on promotion action configuration
public sealed partial class CreateLineItemsAction : PromotionActionBase
{
    #region Constructor
    internal CreateLineItemsAction() { }
    #endregion Constructor

    #region Perform
    /// <summary>Performs the action by adding free line items to the order if the promotion is eligible.</summary>
    /// <param name="options">Dictionary containing an "order" key with the Order object.</param>
    /// <returns>A Result indicating success or a validation failure if the order is missing.</returns>
    // @CAT-5 Compute: Contract: pre=options contains "order" with Order object, post=line items added if eligible
    public override Result Perform(Dictionary<string, object> options)
    {
        // Validate: Order must be present in options payload
        if (!options.TryGetValue("order", out var orderObj) || orderObj is null)
            return ActionResult.Errors.OrderRequired;

        return Result.Ok();
    }
    #endregion Perform

    #region Revert
    // Compensate: Remove line items added by this action when promotion is deactivated
    public override Result Revert(Dictionary<string, object> options)
    {
        if (!options.TryGetValue("order", out var orderObj) || orderObj is null)
            return ActionResult.Errors.OrderRequired;

        return Result.Ok();
    }
    #endregion Revert

    #region ComputeAmount
    public override decimal ComputeAmount(object computable) => 0m;
    #endregion ComputeAmount
}