// GetCartReservations.Response.cs
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Storefront.StockReservations.Get;

public static partial class GetCartReservations
{
    public sealed record CartReservationStatus
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public Guid? StockLocationId { get; init; }
        public Guid? OrderId { get; init; }
        public int Quantity { get; init; }
        public ReservationState State { get; init; }
        public DateTimeOffset? ExpiresAtUtc { get; init; }
        public string? Reason { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset? ModifiedAtUtc { get; init; }
        public int RemainingSeconds { get; init; }
    }
}
