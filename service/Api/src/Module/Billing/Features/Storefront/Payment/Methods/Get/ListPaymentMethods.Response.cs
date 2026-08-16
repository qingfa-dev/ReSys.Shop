using Module.Billing.Features.Storefront.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.Methods;

public static partial class GetPaymentMethods
{
    public sealed record Response : StorePaymentMethodListItemResponse;
}
