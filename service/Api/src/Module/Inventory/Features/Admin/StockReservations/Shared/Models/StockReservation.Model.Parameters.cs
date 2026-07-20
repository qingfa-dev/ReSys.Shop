using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Features.Admin.StockReservations.Shared.Models;

public abstract record StockReservationParameters
{
    /// <summary>Gets or sets the product variant identifier for which stock is reserved.</summary>
    public Guid VariantId { get; init; }

    /// <summary>Gets or sets the stock location identifier from which stock is reserved.</summary>
    public Guid? StockLocationId { get; init; }

    /// <summary>Gets or sets the order identifier for which the reservation was made.</summary>
    public Guid? OrderId { get; init; }

    /// <summary>Gets or sets the quantity of stock reserved. Must be greater than zero.</summary>
    public int Quantity { get; init; }

    /// <summary>Gets or sets the current state of the reservation.</summary>
    public ReservationState State { get; init; } = ReservationState.Reserved;

    /// <summary>Gets or sets the UTC timestamp when this reservation expires.</summary>
    public DateTimeOffset? ExpiresAtUtc { get; init; }

    /// <summary>Gets or sets the reason for creating this reservation.</summary>
    public string? Reason { get; init; }
}