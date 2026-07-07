using Module.Payment.Domain.Payments;

using PaymentDomain = Module.Payment.Domain.Payments.Payment;

namespace Module.Payment.Features.Admin.Payments.Shared.Mappings;

public static class PaymentDomainMapping
{
    public static PaymentDomain MapToDomain<T>(this T parameters) where T : Models.PaymentParameters
    {
        return PaymentExtensions.Create(
            parameters.Amount,
            parameters.PaymentMethodId,
            parameters.OrderId).Value;
    }
}
