// Context: Payment state machine — Checkout→Processing→Pending→Completed; or →Failed/Void→Invalid
namespace Module.Billing.Domain.PaymentCaptures;

public enum PaymentRecordState
{
    Checkout,
    Processing,
    Pending,
    Completed,
    Failed,
    Void,
    Disputed,
    Invalid
}