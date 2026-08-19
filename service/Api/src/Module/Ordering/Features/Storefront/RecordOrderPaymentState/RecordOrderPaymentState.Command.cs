namespace Module.Ordering.Features.Storefront.RecordOrderPaymentState;

/// <summary>
/// Mirrors a payment state transition onto the owning order's payment timestamps.
/// Sent from the Billing module (webhook job / admin capture) via ISender so the
/// Orders timeline stays consistent with the authoritative payment record.
/// </summary>
public sealed record RecordOrderPaymentStateCommand : ICommand
{
    public Guid OrderId { get; init; }
    /// <summary>One of <see cref="PaymentTimelineState"/> values.</summary>
    public PaymentTimelineState PaymentState { get; init; }
    /// <summary>The UTC instant the payment reached this state.</summary>
    public DateTimeOffset AtUtc { get; init; }
}

/// <summary>Payment timeline event kind mirrored onto the owning order's timestamps.</summary>
public enum PaymentTimelineState
{
    Completed,
    Failed,
    Processing
}
