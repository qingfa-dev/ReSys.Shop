namespace Module.Payment.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public sealed record ConfirmPaymentRequest
    {
        public Guid? PaymentMethodId { get; init; }
    }
}
