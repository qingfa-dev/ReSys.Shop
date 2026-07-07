namespace Module.Ordering.Domain.Orders;

/// <summary>
/// Immutable value object representing a payment snapshot for calculation and state tracking.
/// </summary>
// Invariant: Amount >= 0; State is one of 'checkout'/'pending'/'completed'/'failed'/'void'/'invalid'
public record PaymentRecord(decimal Amount, string State, bool IsStoreCredit)
{
    public bool HasInvalidState => State is "void" or "failed" or "invalid";
}
