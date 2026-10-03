using Module.Inventory.Domain.StockItems;
using Module.Inventory.Domain.StockLocations;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Models;

namespace Module.Inventory.Domain.StockMovements;

/// <summary>
/// Records a quantity change (movement) for a stock item, tracking the delta and originator.
/// </summary>
// Invariant: Quantity != 0; OriginatorType is one of: Order, Transfer, Adjustment
// Contract: pre=stockItemId!=Guid.Empty, post=CreatedAtUtc is set
public sealed partial class StockMovement : Entity, IAuditable
{
    #region Properties
    /// <summary>Gets or sets the quantity moved. Positive for received stock, negative for shipped stock.</summary>
    public int Quantity { get; set; }
    /// <summary>Gets or sets the count-on-hand before this movement occurred.</summary>
    public int PreviousCountOnHand { get; set; }
    /// <summary>Gets or sets the action type that caused the movement (e.g., adjust, restock, pick).</summary>
    public string? Action { get; set; }
    /// <summary>Gets or sets the stock item identifier associated with this movement.</summary>
    public Guid StockItemId { get; set; }
    /// <summary>Gets or sets the stock location identifier where the movement occurred.</summary>
    public Guid? StockLocationId { get; set; }
    /// <summary>Gets or sets the identifier of the originator entity (order, transfer, etc.).</summary>
    public Guid? OriginatorId { get; set; }
    /// <summary>Gets or sets the originator type: Order, Transfer, or Adjustment.</summary>
    public string? OriginatorType { get; set; }
    /// <summary>Gets or sets the reason for the movement.</summary>
    public string? Reason { get; set; }
    #endregion Properties

    #region Navigation
    public StockItem? StockItem { get; set; }
    public StockLocation? StockLocation { get; set; }
    #endregion Navigation

    #region Auditing
    /// <summary>Gets or sets the UTC timestamp when this movement was recorded.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
    /// <summary>Gets or sets the UTC timestamp when this movement was last modified.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    /// <summary>Gets or sets the user who recorded this movement.</summary>
    public string? CreatedBy { get; set; }
    /// <summary>Gets or sets the user who last modified this movement.</summary>
    public string? ModifiedBy { get; set; }
    #endregion Auditing

    #region Constructor
    internal StockMovement() { }
    #endregion Constructor
}