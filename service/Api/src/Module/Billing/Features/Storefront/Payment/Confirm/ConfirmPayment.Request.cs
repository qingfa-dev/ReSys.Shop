namespace Module.Billing.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public sealed record Request
    {
        public Guid PaymentId { get; init; }
        public Guid? PaymentMethodId { get; init; }
    }
}
