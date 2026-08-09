using Module.Billing.Features.Storefront.Payment.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntent
{
    public record Response : StorePaymentDetailResponse;
}
