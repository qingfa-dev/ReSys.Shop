namespace Module.Ordering.Domain.Orders;

public static class PaymentRecordConstant
{
    public static class Constraints
    {
        public const int MaxStateLength = 20;
    }

    public static class States
    {
        public const string Checkout = "checkout";
        public const string Pending = "pending";
        public const string Completed = "completed";
        public const string Failed = "failed";
        public const string Void = "void";
        public const string Invalid = "invalid";
    }
}
