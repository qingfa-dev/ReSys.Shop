using Module.Inventory.Features.Admin.StockReservations.Shared.Models;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

public static partial class ReserveCartStock
{
    public sealed class Request : StockReservationRequest
    {
        public int TtlMinutes { get; set; } = 15;
    }
}
