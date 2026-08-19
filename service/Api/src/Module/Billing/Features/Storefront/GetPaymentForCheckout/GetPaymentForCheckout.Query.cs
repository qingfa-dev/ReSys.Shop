namespace Module.Billing.Features.Storefront.GetPaymentForCheckout;

public sealed record GetPaymentForCheckoutQuery : IQuery<PaymentForCheckoutResponse>
{
    public string PaymentIntentId { get; init; } = default!;
    public Guid OrderId { get; init; }
}
