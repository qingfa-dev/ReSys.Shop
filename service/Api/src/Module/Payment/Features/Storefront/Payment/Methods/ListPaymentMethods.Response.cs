using Module.Payment.Features.Storefront.PaymentMethods.Shared.Models;

namespace Module.Payment.Features.Storefront.Payment.Methods;

public static partial class ListPaymentMethods
{
    public sealed record Response : StorePaymentMethodListItemResponse;
}
