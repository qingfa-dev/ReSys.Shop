namespace Module.Promotions.Features.Shared;

public static partial class PromotionsFeature
{
    public static class Storefront
    {
        public const string Route = "api/storefront";

        public static class Promotions
        {
            public const string BaseRoute = $"{Route}/promotions";

            public static class ListActive
            {
                public const string Route = BaseRoute;
                public const string Description = "List active promotions for the current store";
                public const string Summary = "List active promotions";
            }
        }

        public static class Cart
        {
            public const string BaseRoute = $"{Route}/cart";

            public static class ApplyCoupon
            {
                public const string Route = $"{BaseRoute}/coupon";
                public const string Description = "Apply a coupon code to the cart";
                public const string Summary = "Apply coupon";
            }

            public static class RemoveCoupon
            {
                public const string Route = $"{BaseRoute}/coupon";
                public const string Description = "Remove the coupon from the cart";
                public const string Summary = "Remove coupon";
            }
        }
    }
}
