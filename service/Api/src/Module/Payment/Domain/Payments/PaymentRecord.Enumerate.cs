namespace Module.Payment.Domain.Payments;

public enum PaymentState
{
    Checkout,
    Processing,
    Pending,
    Completed,
    Failed,
    Void,
    Invalid
}
