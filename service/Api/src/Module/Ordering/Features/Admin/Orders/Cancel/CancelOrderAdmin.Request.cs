namespace Module.Ordering.Features.Admin.Orders.Cancel;

public static partial class CancelOrderAdmin
{
    public record Request
    {
        /// <summary>Optional free-text reason for the cancellation — recorded for audit trail.</summary>
        public string? Reason { get; init; }
    }
}
