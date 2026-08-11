using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Shared;

public static partial class InventoryFeature
{
    public static class Storefront
    {
        public static class StockItems
        {
            public static class GetAvailability
            {
                public const string Route = "api/storefront/inventory/stock-items/{variantId:guid}/availability";
                public const string Description = "Get per-location stock availability for a variant with optional cart token exclusion";
                public const string Summary = "Get variant stock availability";
            }
        }

        public static class StockReservations
        {
            public const int TtlMinutesDefault = StockReservationConstant.Defaults.DefaultTtlMinutes;

            public static class Reserve
            {
                public const string Route = "api/storefront/inventory/stock-reservations";
                public const string Description = "Reserve stock for a specific variant and location";
                public const string Summary = "Reserve stock";
            }

            public static class Get
            {
                public const string Route = "api/storefront/inventory/stock-reservations";
                public const string Description = "List active stock reservations for the current cart";
                public const string Summary = "Get cart reservations";
            }

            public static class Release
            {
                public const string Route = "api/storefront/inventory/stock-reservations/{id:guid}";
                public const string Description = "Release a single stock reservation by identifier";
                public const string Summary = "Release reservation";
            }
        }
    }
}
