using Module.Payment.Domain.PaymentCaptures;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Shared.Mappings;

public static class PaymentRecordMapping
{
    public static PaymentCapture MapToDomain<T>(this T parameters) where T : Models.PaymentParameters
    {
        return PaymentCaptureMethod.Create(
            parameters.Amount,
            parameters.PaymentMethodId,
            parameters.OrderId).Value;
    }
}