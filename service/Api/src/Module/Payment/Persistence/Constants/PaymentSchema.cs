using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Persistence.Constants;

public static class PaymentSchema
{
    public const string Name = "payment";

    public static class TableNames
    {
        public static string Payments => nameof(Payment).ToSnakeCase()!;
        public static string PaymentMethods => nameof(PaymentMethod).ToSnakeCase()!;
    }
}
