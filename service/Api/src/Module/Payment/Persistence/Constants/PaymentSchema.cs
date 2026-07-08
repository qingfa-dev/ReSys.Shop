using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Domain.Payments;

namespace Module.Payment.Persistence.Constants;

public static class PaymentSchema
{
    public const string Name = "payment";

    public static class TableNames
    {
        public static string PaymentRecords => nameof(PaymentRecord).ToSnakeCase();
        public static string PaymentMethods => nameof(PaymentMethod).ToSnakeCase();
    }
}
