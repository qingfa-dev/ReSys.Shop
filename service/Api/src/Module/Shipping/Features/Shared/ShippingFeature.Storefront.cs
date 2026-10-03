namespace Module.Shipping.Features.Shared;

public static partial class ShippingFeature
{
    public static class Storefront
    {
        public static class Shipping
        {
            public static class Methods
            {
                public const string Route = "api/storefront/shipping/methods";
                public const string Description = "Retrieve available shipping methods";
                public const string Summary = "Get shipping methods";
            }

            public static class Calculate
            {
                public const string Route = "api/storefront/shipping/calculate";
                public const string Description = "Calculate shipping cost for an order and method";
                public const string Summary = "Calculate shipping cost";
            }

            public static class Rates
            {
                public const string Route = "api/storefront/shipping/rates";
                public const string Description = "Retrieve available shipping rates for checkout";
                public const string Summary = "Get shipping rates";
            }
        }
    }
}
