namespace Module.Payment.Domain.PaymentCaptureEvents;

public static class PaymentCaptureEventExtensions
{
    /// <summary>
    /// Creates a new payment capture event recording a captured amount against a payment.
    /// </summary>
    /// <param name="amount">The captured amount. Must be greater than zero.</param>
    /// <param name="paymentId">The identifier of the associated payment.</param>
    /// <returns>A result containing the created payment capture event or a validation error.</returns>
    // Contract: pre=amount>0 && paymentId!=default, post=paymentCaptureEvent.Id!=default && paymentCaptureEvent.Amount==amount
    public static Result<PaymentCaptureEvent> Create(decimal amount, Guid paymentId)
    {
        // Validate: Capture amount must be greater than zero
        if (amount <= 0)
            return PaymentCaptureEventResult.Errors.InvalidAmount;

        return new PaymentCaptureEvent
        {
            Amount = amount,
            PaymentId = paymentId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
    }
}