using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Inventory.Domain.StockReservations;

/// <summary>
/// Represents a temporary reservation of stock quantity for an order or other purpose.
/// </summary>
// Invariant: Quantity > 0; State transitions Reserved -> Fulfilled | Released | Expired
// Contract: pre=variantId!=Guid.Empty && quantity>0, post=ExpiresAtUtc is set for Reserved
public sealed partial class StockReservation : Entity, IAuditable
{
    #region Properties
    /// <summary>Gets or sets the product variant identifier for which stock is reserved.</summary>
    public Guid VariantId { get; set; }
    /// <summary>Gets or sets the stock location identifier from which stock is reserved.</summary>
    public Guid? StockLocationId { get; set; }
    /// <summary>Gets or sets the order identifier for which the reservation was made.</summary>
    public Guid? OrderId { get; set; }
    /// <summary>Gets or sets the quantity of stock reserved. Must be greater than zero.</summary>
    public int Quantity { get; set; }
    /// <summary>Gets or sets the current state of the reservation (Reserved, Fulfilled, Released, Expired).</summary>
    public ReservationState State { get; set; } = ReservationState.Reserved;
    /// <summary>Gets or sets the UTC timestamp when this reservation expires.</summary>
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    /// <summary>Gets or sets the cart token for cart-level reservations.</summary>
    public string? CartToken { get; set; }
    /// <summary>Gets or sets the line item identifier for cart-level reservations.</summary>
    public Guid? LineItemId { get; set; }
    /// <summary>Gets or sets the reason for creating this reservation.</summary>
    public string? Reason { get; set; }
    #endregion Properties

    #region Auditing
    /// <summary>Gets or sets the UTC timestamp when this reservation was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
    /// <summary>Gets or sets the UTC timestamp when this reservation was last modified.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    /// <summary>Gets or sets the user who created this reservation.</summary>
    public string? CreatedBy { get; set; }
    /// <summary>Gets or sets the user who last modified this reservation.</summary>
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal StockReservation() { }
    #endregion Constructor
}

// Reservation lifecycle states
public enum ReservationState
{
    Reserved,
    Fulfilled,
    Released,
    Expired
}