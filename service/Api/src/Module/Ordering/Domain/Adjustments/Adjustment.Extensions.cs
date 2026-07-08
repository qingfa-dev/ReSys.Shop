namespace Module.Ordering.Domain.Adjustments;

public static class AdjustmentMethod
{
    /// <summary>
    /// Creates a new Adjustment with the specified properties.
    /// </summary>
    /// <param name="label">Display label for the adjustment.</param>
    /// <param name="amount">Monetary amount of the adjustment.</param>
    /// <param name="adjustableId">Identifier of the adjustable entity.</param>
    /// <param name="adjustableType">Type of the adjustable entity (Order, LineItem, Shipment).</param>
    /// <param name="sourceId">Identifier of the source entity.</param>
    /// <param name="sourceType">Type of the source entity (Shipping).</param>
    /// <param name="orderId">Parent order identifier.</param>
    /// <param name="mandatory">Whether the adjustment is mandatory.</param>
    /// <param name="eligible">Whether the adjustment is eligible for totals.</param>
    /// <param name="included">Whether the adjustment is included in pricing.</param>
    /// <returns>A successful result containing the new Adjustment.</returns>
    // Contract: pre=label!=null&&amount!=null, post=entity.Id!=null&&entity.State=="open", throws=ArgumentException
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
        // Compute: Format display amount as fixed-point string for presentation
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
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
    }

    /// <summary>
    /// Marks the adjustment as ineligible for totals calculation.
    /// </summary>
    /// <param name="adjustment">The adjustment to modify.</param>
    /// <returns>A success result.</returns>
    // Enforce: Do not modify closed adjustments — state lock prevents recalculation drift
    public static Result MarkIneligible(this Adjustment adjustment)
    {
        if (!adjustment.Eligible)
        {
            return Result.Ok();
        }

        adjustment.Eligible = false;

        return Result.Ok();
    }

    /// <summary>
    /// Marks the adjustment as eligible for totals calculation.
    /// </summary>
    /// <param name="adjustment">The adjustment to modify.</param>
    /// <returns>A success result.</returns>
    // Enforce: Do not modify closed adjustments — state lock prevents recalculation drift
    public static Result MarkEligible(this Adjustment adjustment)
    {
        if (adjustment.Eligible)
        {
            return Result.Ok();
        }

        adjustment.Eligible = true;

        return Result.Ok();
    }

    /// <summary>
    /// Closes the adjustment, preventing further automatic updates.
    /// </summary>
    /// <param name="adjustment">The adjustment to close.</param>
    /// <returns>A success result with confirmation.</returns>
    // @CAT-4 Enforce: Closed adjustments are excluded from recalculation; prevent modifications on closed adjustments — finalised amounts must not change
    public static Result Close(this Adjustment adjustment)
    {
        if (adjustment.State == "closed")
        {
            return AdjustmentResult.Errors.AlreadyClosed;
        }

        // Enforce: Lock adjustment amount by transitioning to closed state
        adjustment.State = "closed";

        return Result.Ok(AdjustmentResult.Success.Closed);
    }

    /// <summary>
    /// Re-opens a closed adjustment, allowing automatic updates again.
    /// </summary>
    /// <param name="adjustment">The adjustment to open.</param>
    /// <returns>A success result with confirmation.</returns>
    // @CAT-4 Enforce: Only re-open adjustments that are currently closed; open adjustments auto-recalculate
    public static Result Open(this Adjustment adjustment)
    {
        if (adjustment.State == "open")
        {
            return AdjustmentResult.Errors.AlreadyOpen;
        }

        // Enforce: Unlock adjustment amount by transitioning to open state
        adjustment.State = "open";

        return Result.Ok(AdjustmentResult.Success.Opened);
    }
}
