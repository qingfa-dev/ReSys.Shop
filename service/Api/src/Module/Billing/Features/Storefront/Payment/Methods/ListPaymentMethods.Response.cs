using Module.Billing.Features.Storefront.PaymentMethods.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.Methods;

public static partial class ListPaymentMethods
{
    public sealed record Response : StorePaymentMethodListItemResponse;
}
