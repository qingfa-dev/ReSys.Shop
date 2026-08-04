namespace Module.Inventory.Features.Storefront.CartReservations.Release;

public static partial class ReleaseCartReservation
{
    public sealed record Response
    {
        public Guid ReservationId { get; init; }
        public string Status { get; init; } = "released";
    }
}
