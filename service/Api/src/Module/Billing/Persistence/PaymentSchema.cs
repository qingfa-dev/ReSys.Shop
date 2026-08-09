namespace Module.Billing.Persistence;

public static class PaymentSchema
{
    public const string Name = "payment";

    public static class TableNames
    {
        public const string PaymentCaptures = "payment_captures";
        public const string PaymentMethods = "payment_methods";
    }
}
