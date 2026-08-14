namespace Module.Shipping.Domain.ShippingMethods;


public static class ShippingMethodConstant
{
    // Validate: Enforce domain constraints for shipping method fields
    public static class Constraints
    {
        public const int MaxNameLength = 255;
        public const int MaxCodeLength = 50;
        public const int MaxTrackingUrlLength = 2048;
        public const int MaxAdminNameLength = 255;
        public const int MaxCalculatorTypeLength = 100;
    }

    public static class Defaults
    {
        public const bool AvailableToUsers = true;
        public const int Position = 0;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(ShippingMethod.Name),
            nameof(ShippingMethod.Code),
            nameof(ShippingMethod.AdminName)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(ShippingMethod.Name),
            nameof(ShippingMethod.Code),
            nameof(ShippingMethod.Position),
            nameof(ShippingMethod.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(ShippingMethod.AvailableToUsers),
            nameof(ShippingMethod.CalculatorType),
            nameof(ShippingMethod.IsDeleted)
        ];
    }
}