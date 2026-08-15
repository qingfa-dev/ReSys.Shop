using Module.Billing.Features.Admin.Payments.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.Shared.Models;

public record StorePaymentRequest : PaymentParameters;

public abstract record PaymentConfirmationParameters
{
    public Guid PaymentId { get; init; }
    public Guid? PaymentMethodId { get; init; }
}
