namespace Module.Billing.Domain.PaymentCaptures;

public static partial class PaymentCaptureMethod
{
    #region Factory Methods
    // Create: PaymentCapture entity with Checkout state and auto-generated number
    public static Result<PaymentCapture> Create(
        decimal amount,
        Guid paymentMethodId,
        Guid orderId,
        string? sourceId = null,
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