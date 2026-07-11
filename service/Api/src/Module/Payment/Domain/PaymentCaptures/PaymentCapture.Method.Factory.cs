using Module.Payment.Services.Abstractions;
using Module.Payment.Services.Models;
using Module.Payment.Services.Gateways;

namespace Module.Payment.Domain.PaymentCaptures;

public static partial class PaymentCaptureMethod
{
    #region Factory Methods
    public static Result<PaymentCapture> Create(
        decimal amount,
        Guid paymentMethodId,
        Guid orderId,
        Guid? sourceId = null,
        string? sourceType = null)
    {
        if (amount <= 0)
            return PaymentCaptureResult.Failure.AmountMustBePositive;

        var payment = new PaymentCapture
        {
            Id = Guid.NewGuid(),
            Number = GeneratePaymentNumber(),
            Amount = amount,
            State = PaymentRecordState.Checkout,
            PaymentMethodId = paymentMethodId,
            OrderId = orderId,
            SourceId = sourceId,
            SourceType = sourceType,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return payment;
    }

    private static string GeneratePaymentNumber()
    {
        return $"PAY-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
    #endregion
}
