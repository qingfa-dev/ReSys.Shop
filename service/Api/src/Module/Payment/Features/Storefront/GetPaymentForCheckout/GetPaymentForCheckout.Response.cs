namespace Module.Payment.Features.Storefront.GetPaymentForCheckout;

public sealed record PaymentForCheckoutResponse
{
    public decimal Amount { get; init; }
    public bool IsCompleted { get; init; }
}
