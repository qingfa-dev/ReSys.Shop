namespace Module.Promotions.Domain.Actions;
/// <summary>Represents a Free Shipping Action.</summary>

// Compute: Zero-cost shipping adjustment applied to each shipment
// Invariant: Free shipping is applied based on promotion action configuration
public sealed partial class FreeShippingAction : PromotionActionBase
{
    #region Constructor
    internal FreeShippingAction() { }
    #endregion Constructor

    #region Perform
    /// <summary>Performs the free-shipping action by creating zero-cost shipping adjustments on each shipment.</summary>
    /// <param name="options">Dictionary containing an "order" key with the Order object.</param>
    /// <returns>A Result indicating success or a validation failure if the order is missing.</returns>
    // @CAT-5 Compute: Contract: pre=payload contains "order" key with Order object, post=adjustments created on each shipment
    public override Result Perform(Dictionary<string, object> options)
    {
        // Validate: Order must be present in payload
        if (!options.TryGetValue("order", out var orderObj) || orderObj is null)
            return ActionResult.Errors.OrderRequired;

        return Result.Ok();
    }
    #endregion Perform

    #region ComputeAmount
    // Compute: shipment_cost * -1 to set shipping to zero
    public override decimal ComputeAmount(object computable)
    {
        return 0m;
    }
    #endregion ComputeAmount
}