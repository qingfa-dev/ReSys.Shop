using Module.Inventory.Features.Admin.StockReservations.Shared.Models;

namespace Module.Inventory.Features.Storefront.CartReservations.Status;

public static partial class GetCartReservations
{
    public sealed class Response : StockReservationListItemResponse
    {
        public new string State { get; init; } = "Reserved";
        public int RemainingSeconds { get; init; }
    }
}
