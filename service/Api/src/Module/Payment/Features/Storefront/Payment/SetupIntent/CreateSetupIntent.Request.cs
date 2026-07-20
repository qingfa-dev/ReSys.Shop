using Module.Payment.Features.Storefront.Payment.Shared.Models;

namespace Module.Payment.Features.Storefront.Payment.SetupIntent;

public static partial class CreateSetupIntent
{
    public sealed record Request : StorePaymentRequest;
}
