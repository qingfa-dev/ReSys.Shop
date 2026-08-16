using Module.Billing.Features.Admin.Shared.Models;

namespace Module.Billing.Features.Storefront.Shared.Models;

public record StorePaymentMethodListItemResponse : PaymentMethodParameters
{
    public Guid Id { get; init; }
}
