using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Persistence;

// Context: EF Core schema — table names use snake_case convention
public static class PaymentSchema
{
    public const string Name = "payment";

    public static class TableNames
    {
        public static string PaymentRecords => nameof(PaymentCapture).ToSnakeCase();
        public static string PaymentMethods => nameof(PaymentMethod).ToSnakeCase();
    }
}