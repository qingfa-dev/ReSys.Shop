namespace Module.Payment.Domain.PaymentCaptures;

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
        public const int Precision = 18;
        public const int Scale = 2;
    }

    public static class Amount
    {
        public const long CentsMultiplier = 100;
    }

    public static class PaymentNumber
    {
        public const string Prefix = "PAY-";
        public const string DateFormat = "yyyyMMdd";
        public const string Format = $"{Prefix}{{{DateFormat}}}-";
    }

    public static class Defaults
    {
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