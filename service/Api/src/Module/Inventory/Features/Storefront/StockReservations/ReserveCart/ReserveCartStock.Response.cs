namespace Module.Inventory.Features.Storefront.StockReservations.ReserveCart;

public static partial class ReserveCartStock
{
    public sealed record Response
    {
        public IReadOnlyList<Guid> ReservationIds { get; init; } = [];
    }
}
