namespace Module.Inventory.Features.Storefront.CartReservations.ReleaseSingle;

public static partial class ReleaseCartReservation
{
    public sealed record Request
    {
        public Guid ReservationId { get; init; }
        public string CartToken { get; init; } = string.Empty;
    }
}
