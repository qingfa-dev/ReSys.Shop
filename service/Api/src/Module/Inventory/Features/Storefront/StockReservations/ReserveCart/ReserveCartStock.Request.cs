using Module.Inventory.Features.Shared;
using Module.Inventory.Features.Storefront.Shared.Models;

namespace Module.Inventory.Features.Storefront.StockReservations.ReserveCart;

public static partial class ReserveCartStock
{
    public sealed record Request
    {
        public Guid CartId { get; init; }
        public IReadOnlyList<ReserveLineItem> LineItems { get; init; } = [];
        public int TtlMinutes { get; init; } = InventoryFeature.Storefront.StockReservations.TtlMinutesDefault;
    }
}
