using Module.Payment.Features.Storefront.Payment.Shared.Models;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    public record Request : StorePaymentRequest
    {
        public string? ReturnUrl { get; init; }
    }
}
