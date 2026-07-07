namespace Module.Shipping.Domain.ShippingRates;

public enum ShippingRateCategory
{
    Standard,
    Express,
    Overnight
}

public static class ShippingRateConstant
{
    // Validate: Enforce domain constraints for shipping rate fields
    public static class Constraints
    {
        public const int MaxNameLength = 255;
        public const int MaxDisplayPriceLength = 50;
        public const int MaxDeliveryRangeLength = 100;
        public const int Precision = 18;
        public const int Scale = 2;
    }

    public static class Defaults
    {
        public const bool Selected = false;
    }

    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(ShippingRate.Name),
            nameof(ShippingRate.DeliveryRange)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(ShippingRate.Name),
            nameof(ShippingRate.Cost),
            nameof(ShippingRate.FinalPrice),
            nameof(ShippingRate.Selected),
            nameof(ShippingRate.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(ShippingRate.Selected),
            nameof(ShippingRate.ShippingMethodId)
        ];
    }
}