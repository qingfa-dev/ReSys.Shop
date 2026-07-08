using Module.Payment.Domain.Payments;

using PaymentRecord = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.Payment.Features.Admin.Payments.Shared.Mappings;

public static class PaymentRecordMapping
{
    public static PaymentRecord MapToDomain<T>(this T parameters) where T : Models.PaymentParameters
    {
        return PaymentFactory.Create(
            parameters.Amount,
            parameters.PaymentMethodId,
            parameters.OrderId).Value;
    }
}
