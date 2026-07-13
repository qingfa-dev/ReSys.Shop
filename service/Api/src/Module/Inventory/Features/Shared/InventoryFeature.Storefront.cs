namespace Module.Inventory.Features.Shared;

public static partial class InventoryFeature
{
    public static class Storefront
    {
        public const string Route = "api/storefront";

        public static class StockAvailability
        {
            public const string BaseRoute = $"{Route}/availability";

            public static class Check
            {
                public const string Route = $"{BaseRoute}/{{variantId:guid}}";
                public const string Description = "Check stock availability for a variant across all stock locations";
                public const string Summary = "Check variant availability";
            }
        }

        public static class CartReservations
        {
            public const string BaseRoute = $"{Route}/cart/reserve";

            public static class Reserve
            {
                public const string Route = BaseRoute;
                public const string Description = "Reserve stock for a cart item with configurable TTL";
                public const string Summary = "Reserve cart stock";
            }

            public static class Release
            {
                public const string Route = $"{BaseRoute}/{{reservationId:guid}}";
                public const string Description = "Release a cart stock reservation";
                public const string Summary = "Release cart reservation";
            }

            public static class Status
            {
                public const string Route = BaseRoute;
                public const string Description = "Get active stock reservations for the current cart";
                public const string Summary = "Get cart reservations";
            }
        }
    }
}