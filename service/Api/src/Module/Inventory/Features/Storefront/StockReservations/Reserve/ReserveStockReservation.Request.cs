using Module.Inventory.Features.Storefront.StockReservations.Shared.Models;

namespace Module.Inventory.Features.Storefront.StockReservations.Reserve;

public static partial class ReserveStockReservation
{
    public sealed record Request : StockReservationReserveParameters;
}
