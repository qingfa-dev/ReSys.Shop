namespace Module.Payment.Domain.PaymentCaptures;

// Context: Domain constraints, patterns, defaults, and query configuration for PaymentCapture
public static class PaymentConstant
{
    public static class Constraints
    {
        public const int MaxPaymentNumberLength = 50;
        public const int MaxNumberLength = 50;
        public const int MaxResponseCodeLength = 255;
        public const int MaxAvsResponseLength = 255;
        public const int MaxCvvCodeLength = 10;
        public const int MaxCvvMessageLength = 255;
        public const int MaxSourceTypeLength = 100;
        public const int MaxIntentClientSecretLength = 500;
        public const int MaxProviderKeyLength = 50;
        public const int MaxCurrencyLength = 3;
        public const int Precision = 18;
        public const int Scale = 2;
    }

    public static class Patterns
    {
        public const string Number = @"^PAY-\d{8}-[A-Z0-9]+$";
    }

    public static class Amount
    {
        public const long CentsMultiplier = 100;
    }

    public static class RefundedAmount
    {
        public const decimal MinValue = 0;
    }

    public static class PaymentNumber
    {
        public const string Prefix = "PAY-";
        public const string DateFormat = "yyyyMMdd";
        public const string Format = $"{Prefix}{{{DateFormat}}}-";
    }

    public static class Defaults
    {
        public const string Currency = "USD";
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(PaymentCapture.Number)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(PaymentCapture.Number),
            nameof(PaymentCapture.Amount),
            nameof(PaymentCapture.State),
            nameof(PaymentCapture.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(PaymentCapture.State),
            nameof(PaymentCapture.PaymentMethodId),
            nameof(PaymentCapture.OrderId)
        ];
    }
}