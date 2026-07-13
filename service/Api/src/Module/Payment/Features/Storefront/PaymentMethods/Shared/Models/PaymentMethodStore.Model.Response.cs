using Module.Payment.Features.Admin.PaymentMethods.Shared.Models;

namespace Module.Payment.Features.Storefront.PaymentMethods.Shared.Models;

public record StorePaymentMethodListItemResponse : PaymentMethodParameters
{
    public Guid Id { get; init; }
}
