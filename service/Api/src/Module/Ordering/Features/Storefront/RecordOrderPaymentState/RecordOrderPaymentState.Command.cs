namespace Module.Ordering.Features.Storefront.RecordOrderPaymentState;

/// <summary>
/// Mirrors a payment state transition onto the owning order's payment timestamps.
/// Sent from the Billing module (webhook job / admin capture) via ISender so the
/// Orders timeline stays consistent with the authoritative payment record.
/// </summary>
public sealed record RecordOrderPaymentStateCommand : ICommand
{
    public Guid OrderId { get; init; }
    /// <summary>One of <see cref="OrderPaymentState"/> values (PaymentRecordState name).</summary>
    public string PaymentState { get; init; } = default!;
    /// <summary>The UTC instant the payment reached this state.</summary>
    public DateTimeOffset AtUtc { get; init; }
}

/// <summary>Payment state names accepted by <see cref="RecordOrderPaymentStateCommand"/> (mirror of PaymentRecordState).</summary>
public static class OrderPaymentState
{
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Processing = "Processing";
}
