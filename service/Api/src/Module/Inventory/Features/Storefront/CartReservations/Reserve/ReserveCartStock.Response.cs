using Module.Inventory.Features.Admin.StockReservations.Shared.Models;

namespace Module.Inventory.Features.Storefront.CartReservations.Reserve;

public static partial class ReserveCartStock
{
    public sealed class Response : StockReservationDetailResponse
    {
        public new string State { get; init; } = "Reserved";
    }
}