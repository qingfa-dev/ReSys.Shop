namespace Module.Ordering.Features.Admin.Orders.Update.Adjustment;

public static partial class UpdateOrderAdjustment
{
    public enum AdjustmentAction
    {
        Close,
        Open,
        MarkEligible,
        MarkIneligible
    }

    public class Request
    {
        public AdjustmentAction Action { get; init; }
    }
}
