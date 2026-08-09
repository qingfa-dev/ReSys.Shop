using Module.Billing.Features.Admin.PaymentMethods.Shared.Models;

namespace Module.Billing.Features.Storefront.PaymentMethods.Shared.Models;

public record StorePaymentMethodListItemResponse : PaymentMethodParameters
{
    public Guid Id { get; init; }
}
