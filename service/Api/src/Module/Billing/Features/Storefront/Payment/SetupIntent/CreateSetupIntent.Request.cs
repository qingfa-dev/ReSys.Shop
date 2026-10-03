using Module.Billing.Features.Storefront.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.SetupIntent;

public static partial class CreateSetupIntent
{
    public sealed record Request : StorePaymentRequest;
}
