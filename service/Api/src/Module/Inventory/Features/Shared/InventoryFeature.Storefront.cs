using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Shared;

public static partial class InventoryFeature
{
    public static class Storefront
    {
        public static class StockReservations
        {
            public const int TtlMinutesDefault = StockReservationConstant.Defaults.DefaultTtlMinutes;
        }
    }
}
