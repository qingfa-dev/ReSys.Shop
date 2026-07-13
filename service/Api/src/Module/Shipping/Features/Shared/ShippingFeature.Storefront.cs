namespace Module.Shipping.Features.Shared;

public static partial class ShippingFeature
{
    public static class Storefront
    {
        public const string Route = "api/storefront";

        public static class Shipping
        {
            public const string BaseRoute = $"{Route}/shipping";

            public static class Methods
            {
                public const string Route = $"{BaseRoute}/methods";
                public const string Description = "Retrieve available shipping methods";
                public const string Summary = "Get shipping methods";
            }

            public static class Calculate
            {
                public const string Route = $"{BaseRoute}/calculate";
                public const string Description = "Calculate shipping cost for an order";
                public const string Summary = "Calculate shipping";
            }

            public static class Rates
            {
                public const string Route = $"{BaseRoute}/rates";
                public const string Description = "Retrieve available shipping rates for checkout";
                public const string Summary = "Get shipping rates";
            }
        }
    }
}