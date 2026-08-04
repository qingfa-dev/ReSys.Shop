using Module.Payment.Features.Storefront.Payment.Shared.Models;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    public record Request : StorePaymentRequest
    {
        public string? ReturnUrl { get; init; }
        public new Guid? PaymentMethodId { get; init; }
        // Gateway token for SourceRequired gateways (Stripe: pm_... tokens, Bogus: test card number)
        public string? PaymentMethodToken { get; init; }
    }
}
