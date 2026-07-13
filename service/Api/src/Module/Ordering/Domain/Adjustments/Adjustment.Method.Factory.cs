namespace Module.Ordering.Domain.Adjustments;

// Create: Factory methods for producing new Adjustment entities with audit defaults
public static partial class AdjustmentMethod
{
    #region Factory Methods
    // Contract: pre=label!=null && amount>=0 && adjustableId!=default && sourceId!=default && orderId!=default,
    //           post=Adjustment.State=="open" && Adjustment.Eligible==eligible && DisplayAmount matches "F2"
    public static Result<Adjustment> Create(
        string label,
        decimal amount,
        Guid adjustableId,
        string adjustableType,
        Guid sourceId,
        string sourceType,
        Guid orderId,
        bool mandatory = AdjustmentConstant.Defaults.Mandatory,
        bool eligible = AdjustmentConstant.Defaults.Eligible,
        bool included = AdjustmentConstant.Defaults.Included)
    {
        // Format: DisplayAmount uses invariant "F2" — prevents locale-driven decimal separators in API payloads
        // Create: state defaults to "open" so downstream recalculations pick up this adjustment
        return new Adjustment
        {
            Label = label,
            Amount = amount,
            DisplayAmount = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            Eligible = eligible,
            Included = included,
            Mandatory = mandatory,
            State = AdjustmentConstant.Defaults.State,
            AdjustableId = adjustableId,
            AdjustableType = adjustableType,
            SourceId = sourceId,
            SourceType = sourceType,
            OrderId = orderId,
            // Assign: UTC timestamp + "System" principal — audit trail for all adjustments regardless of origin
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = AdjustmentConstant.Defaults.CreatedBy
        };
    }
    #endregion
}
