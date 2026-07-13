namespace Module.Payment.Domain.PaymentMethods;

// Context: Domain constraints, defaults, patterns, and query configuration for PaymentMethod
public static class PaymentMethodConstant
{
    public static class Constraints
    {
        public const int MaxNameLength = 255;
        public const int MaxCodeLength = 50;
        public const int MaxDescriptionLength = 1000;
        public const int MaxProviderKeyLength = 50;
        public const int MaxPresentationLength = 500;
        public const int MinPositionValue = 0;
        public const int MaxPositionValue = 9999;
        public const int MaxSettingsItems = 50;
        public const int MaxSettingsKeyLength = 100;
        public const int MaxSettingsValueLength = 2000;
        public const int MaxPreferencesItems = 50;
        public const int MaxPreferencesKeyLength = 100;
        public const int MaxPreferencesValueLength = 2000;
    }

    public static class Defaults
    {
        public const bool Active = true;
        public const bool AutoCapture = false;
        public const int Position = 0;
    }

    public static class Patterns
    {
        public const string Code = @"^[a-zA-Z0-9_-]+$";
    }

    public static class Code
    {
        public const int MinLength = 1;
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
            nameof(PaymentMethod.ProviderKey),
            nameof(PaymentMethod.AutoCapture),
            nameof(PaymentMethod.DisplayOn),
            nameof(PaymentMethod.IsDeleted)
        ];
    }
}