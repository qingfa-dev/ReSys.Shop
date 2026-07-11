namespace Module.Payment.Domain.PaymentCaptures;

public enum PaymentRecordState
{
    Checkout,
    Processing,
    Pending,
    Completed,
    Failed,
    Void,
    Invalid
}
