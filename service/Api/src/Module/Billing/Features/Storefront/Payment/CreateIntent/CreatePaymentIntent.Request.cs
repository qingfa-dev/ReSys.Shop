using Module.Billing.Features.Storefront.Payment.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    public record Request : StorePaymentRequest
    {
        public string? ReturnUrl { get; init; }
        public string? CancelUrl { get; init; }
        public new Guid? PaymentMethodId { get; init; }
        // Gateway token for SourceRequired gateways (Stripe: pm_... tokens, Bogus: test card number)
        public string? PaymentMethodToken { get; init; }
        // Test card number for Bogus gateway demo path (e.g. "4111111111111111")
        public string? CardNumber { get; init; }
    }
}
