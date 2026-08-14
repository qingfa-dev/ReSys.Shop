namespace Module.Billing.Features.Storefront.GetPaymentForCheckout;

public sealed record PaymentForCheckoutResponse
{
    public decimal Amount { get; init; }
    public bool IsCompleted { get; init; }
    public bool IsPending { get; init; }
    public bool IsOffline { get; init; }
    /// <summary>When the payment reached Completed — mirrored onto the order timeline.</summary>
    public DateTimeOffset? CompletedAtUtc { get; init; }
}
