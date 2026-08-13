namespace Module.Billing.Features.Storefront.GetPaymentForCheckout;

public sealed record PaymentForCheckoutResponse
{
    public decimal Amount { get; init; }
    public bool IsCompleted { get; init; }
    public string State { get; init; } = string.Empty;
    public bool IsOffline { get; init; }
}
