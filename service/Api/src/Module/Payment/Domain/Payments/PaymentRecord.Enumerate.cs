namespace Module.Payment.Domain.Payments;

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
