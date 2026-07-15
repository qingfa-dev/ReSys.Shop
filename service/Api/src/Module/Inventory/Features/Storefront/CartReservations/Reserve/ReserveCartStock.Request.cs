using Module.Inventory.Features.Admin.StockReservations.Shared.Models;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

public static partial class ReserveCartStock
{
    public sealed class Request : StockReservationRequest
    {
        public string CartToken { get; set; } = string.Empty;
        public int TtlMinutes { get; set; } = 15;
    }
}