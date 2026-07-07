namespace Module.Payment.Domain.PaymentMethods;

public static class PaymentMethodConstant
{
    public static class Constraints
    {
        public const int MaxNameLength = 255;
        public const int MaxCodeLength = 50;
        public const int MaxDescriptionLength = 1000;
        public const int MaxProviderTypeLength = 100;
    }

    public static class Defaults
    {
        public const bool Active = true;
        public const bool AutoCapture = false;
        public const DisplayOn DisplayOn = Domain.PaymentMethods.DisplayOn.Both;
        public const int Position = 0;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(PaymentMethod.Name),
            nameof(PaymentMethod.Code),
            nameof(PaymentMethod.Description)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(PaymentMethod.Name),
            nameof(PaymentMethod.Position),
            nameof(PaymentMethod.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(PaymentMethod.Active),
            nameof(PaymentMethod.ProviderType),
            nameof(PaymentMethod.AutoCapture),
            nameof(PaymentMethod.DisplayOn),
            nameof(PaymentMethod.IsDeleted)
        ];
    }
}