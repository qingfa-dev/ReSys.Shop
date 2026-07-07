namespace Module.Ordering.Domain.Orders;

public static class PaymentRecordResult
{
    public static class Errors
    {
        public static Error InvalidState => Error.Validation(
            code: "PaymentRecord.State.Invalid",
            description: "Payment state must be one of: checkout, pending, completed, failed, void, invalid.");
    }
}
