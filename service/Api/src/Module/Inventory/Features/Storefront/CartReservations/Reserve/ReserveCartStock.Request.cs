using Module.Inventory.Features.Admin.StockReservations.Shared.Models;
using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

public static partial class ReserveCartStock
{
    public sealed record Request : StockReservationRequest
    {
        public string CartToken { get; set; } = string.Empty;
        public int TtlMinutes { get; set; } = InventoryFeature.Storefront.Cart.TtlMinutesDefault;
    }
}