namespace Module.Payment.Domain.Payments;

public static class PaymentConstant
{
    public static class Constraints
    {
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

    public static class Defaults
    {
        public const PaymentState State = Payments.PaymentState.Checkout;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Payment.Number)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Payment.Number),
            nameof(Payment.Amount),
            nameof(Payment.State),
            nameof(Payment.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Payment.State),
            nameof(Payment.PaymentMethodId),
            nameof(Payment.OrderId)
        ];
    }
}